using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HPCWebsite.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
