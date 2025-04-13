using Microsoft.AspNetCore.Mvc;

namespace HPCWebsite.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Login()
        {

            return View("Login");
        }

        public IActionResult LoginCode(long mobile)
        {
            var code = await _userService.GenerateVerificationCodeAsync(mobile);
            var user = await _userService.LoginAsync(mobile);
            return View("LoginCode", mobile);
        }
        public IActionResult CheckCode(long mobile,string code)
        {
            var result = await _userService.VerifyCodeAsync(mobile, code);
            if (!result) return BadRequest("کد تایید نامعتبر است");
            return View("SignUp",0);
        }
        public IActionResult SignUp(int id, string firstname, string lastname)
        {
            var createdUser = await _userService.CreateAsync(user);
            return View("SignUp");
        }
        
    }
}
