using System.Diagnostics;
using DataLayer.DbContext;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace HPCWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServerService _serverService;

        public HomeController(IServerService serverService)
        {
            _serverService = serverService;
        }

        public async Task<IActionResult> Index()
        {
            var cpuServers = await _serverService.GetServersByTypeAsync(ServerType.CPU);
            var gpuServers = await _serverService.GetServersByTypeAsync(ServerType.GPU);

            var model = new ServerPlansViewModel
            {
                CPUServers = cpuServers,
                GPUServers = gpuServers
            };

            return View(model);
        }
    }
}
