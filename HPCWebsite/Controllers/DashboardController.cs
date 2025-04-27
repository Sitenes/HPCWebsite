using Microsoft.AspNetCore.Mvc;

namespace HPCWebsite.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Checkout()
        {
            return View();
        }
        public IActionResult ServerList()
        {
            return View();
        }
        public IActionResult Server()
        {
            return View();
        }
    }
}
