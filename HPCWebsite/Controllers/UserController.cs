using Entity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Service;
using System.Security.Claims;
using ViewModel;

namespace HPCWebsite.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(
            IUserService userService)
        {
            _userService = userService;
        }
        [AllowAnonymous]
        public IActionResult Login()
        {

            return View("Login");
        }
        [AllowAnonymous]
        public async Task<IActionResult> LoginCodeAsync(long mobile)
        {

            var user = await _userService.GetByMobileAsync(mobile);
            if (user == null)
            {
                user = new User
                {
                    Mobile = mobile,
                };
                user = await _userService.CreateAsync(user);
            }

            var code = await _userService.GenerateVerificationCodeAsync(mobile);
            await _userService.SaveChangesAsync();
            var result = new CodeViewModel
            {
                Mobile = mobile
            };
            return View("LoginCode", result);
        }

        [AllowAnonymous]
        public async Task<IActionResult> CheckCodeAsync(CodeViewModel model)
        {
            var access = await _userService.VerifyCodeAsync(model.Mobile, model.Code);
            await _userService.SaveChangesAsync();



            //if (!access) //Validation Code
            //{
            //var result = new CodeViewModel
            //{
            //    Mobile = model.Mobile,
            //    Code = model.Code
            //};
            //    ModelState.AddModelError(nameof(model.Code), "کد وارد شده معتبر نمی باشد");
            //    return View("LoginCode", result);
            //}

            var user = await _userService.GetByMobileAsync(model.Mobile);
            if (user == null)
                return RedirectToAction("login", "user");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Mobile.ToString()),
                new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, "User"), // نقش پیش‌فرض
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true, // کوکی پایدار
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) // مدت اعتبار
                });


            if (user.FirstName.IsNullOrEmpty() && user.LastName.IsNullOrEmpty() && user.Email.IsNullOrEmpty())
            {
                var result = new SignUpViewModel
                {
                    Id = user.Id,
                };
                return View("SignUp", result);
            }

            return RedirectToAction("index", "dashboard");
        }
        [Authorize]
        [HttpPost(nameof(SignUpAsync))]
        public async Task<IActionResult> SignUpAsync(SignUpViewModel input)
        {
            var createdUser = await _userService.GetByIdAsync(input.Id);

            if (createdUser == null || !createdUser.IsMobileVerified)
            {
                return RedirectToAction("login", "user");
            }

            createdUser.IsActive = true;
            createdUser.FirstName = input.FirstName;
            createdUser.LastName = input.LastName;
            createdUser.Email = input.Email;

            // ذخیره تغییرات در دیتابیس (بسته به سرویس شما ممکن است متد متفاوت باشد)
            _userService.Update(createdUser);
           
            await _userService.SaveChangesAsync();
            // به‌روزرسانی اطلاعات کاربر در کوکی
            await UpdateUserClaims(createdUser);

            return RedirectToAction("Index", "Dashboard");
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index","Home");
        }
        private async Task UpdateUserClaims(User user)
        {
            var identity = (ClaimsIdentity)User.Identity;
            identity.RemoveClaim(identity.FindFirst("FullName"));
            identity.AddClaim(new Claim("FullName", $"{user.FirstName} {user.LastName}"));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
        }
    }
}
