using System.Diagnostics;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace HPCWebsite.Controllers
{
    public class ProductController : Controller
    {
        private readonly IServerService _serverService;

        public ProductController(IServerService serverService)
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

    public class ServerPlansViewModel
    {
        public List<HpcServer> CPUServers { get; set; }
        public List<HpcServer> GPUServers { get; set; }
    }
}
