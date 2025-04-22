using Entity.Users;
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
            var result = new CodeViewModel
            {
                Mobile = model.Mobile,
                Code = model.Code
            };

            //if (!access) //Validation Code
            //{
            //    ModelState.AddModelError(nameof(model.Code), "کد وارد شده معتبر نمی باشد");
            //    return View("LoginCode", result);
            //}

            //var user = await _userService.GetByMobileAsync(model.Mobile);
            // if (user.FirstName.IsNullOrEmpty() && user.LastName && user.Email)
            return View("SignUp", result);
        }

        public async Task<IActionResult> SignUpAsync(long mobile, string firstname, string lastname)
        {
            var user = new User
            {
                Mobile = mobile,
                FirstName = firstname,
                LastName = lastname,
            };
            var createdUser = await _userService.CreateAsync(user);
            return View("SignUp");
        }

    }
}
