using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HPCWebsite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
