using System.Diagnostics;
using Entity;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace HPCWebsite.Controllers
{
    [Route("servers")]
    public class ServersController : Controller
    {
        private readonly IServerService _serverService;
        private readonly ILogger<ServersController> _logger;

        public ServersController(
            IServerService serverService,
            ILogger<ServersController> logger)
        {
            _serverService = serverService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
               
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading servers page");
                return View("Error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var server = await _serverService.GetServerByIdAsync(id);
                if (server == null)
                    return NotFound();

                return View(server);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting server details for id {id}");
                return View("Error");
            }
        }

        [HttpGet("cpu")]
        public async Task<IActionResult> CpuServers()
        {
            try
            {
                var servers = await _serverService.GetServersByTypeAsync(ServerType.CPU);
                return View("ServerList", servers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CPU servers");
                return View("Error");
            }
        }

        [HttpGet("gpu")]
        public async Task<IActionResult> GpuServers()
        {
            try
            {
                var servers = await _serverService.GetServersByTypeAsync(ServerType.GPU);
                return View("ServerList", servers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GPU servers");
                return View("Error");
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> PopularServers()
        {
            try
            {
                var servers = await _serverService.GetPopularServersAsync();
                return View("ServerList", servers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading popular servers");
                return View("Error");
            }
        }

        [HttpGet("{id}/calculate-price")]
        public async Task<IActionResult> CalculatePrice(int id, [FromQuery] int days)
        {
            try
            {
                if (days < 1)
                    return BadRequest("تعداد روزها باید حداقل 1 باشد");

                var price = await _serverService.CalculateRentalPriceAsync(id, days);
                return Ok(new { Price = price, FormattedPrice = price.ToString("N0") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating price for server {id} and days {days}");
                return StatusCode(500, "خطا در محاسبه قیمت");
            }
        }

        [HttpGet("{id}/check-availability")]
        public async Task<IActionResult> CheckAvailability(int id)
        {
            try
            {
                var isAvailable = await _serverService.IsServerAvailableAsync(id);
                return Ok(new { Available = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking availability for server {id}");
                return StatusCode(500, "خطا در بررسی موجودی");
            }
        }
    }
}
