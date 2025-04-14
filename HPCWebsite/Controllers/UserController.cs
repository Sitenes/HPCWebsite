using Entity.Users;
using Microsoft.AspNetCore.Mvc;
using Service;

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
            var code = await _userService.GenerateVerificationCodeAsync(mobile);
            var user = await _userService.LoginAsync(mobile);
            return View("LoginCode", mobile);
        }
        public async Task<IActionResult> CheckCodeAsync(long mobile,string code)
        {
            var result = await _userService.VerifyCodeAsync(mobile, code);
            if (!result) return BadRequest("کد تایید نامعتبر است");
            return View("SignUp",0);
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
