using Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Service;
using ViewModel;

namespace HPCWebsite.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Login()
        {

            return View("Login");
        }

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
        [HttpPost(nameof(SignUpAsync))]
        public async Task<IActionResult> SignUpAsync(SignUpViewModel input)
        {
            var createdUser = await _userService.GetByIdAsync(input.Id);

            if (createdUser == null || !createdUser.IsMobileVerified)
            {
                return RedirectToAction("login", "user");
            }

            var user = new User
            {
                Id = createdUser.Id,
                IsActive = true,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Email = input.Email,
            };
            _userService.Update(user);
            await _userService.SaveChangesAsync();
            return RedirectToAction("index", "dashboard");
        }

    }
}
