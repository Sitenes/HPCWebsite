
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

namespace Service
{
    public interface IServerService
    {
        Task<Server> GetServerByIdAsync(int id);
        Task<List<Server>> GetAllServersAsync();
        Task<List<Server>> GetServersByTypeAsync(ServerType type);
        Task<List<ServerCategory>> GetServerCategoriesAsync();
        Task<Server> CreateServerAsync(Server server);
        Task<Server> UpdateServerAsync(Server server);
        Task<bool> DeleteServerAsync(int id);
        Task<List<Server>> GetPopularServersAsync(int count = 4);
        Task<bool> IsServerAvailableAsync(int serverId);
        Task<decimal> CalculateRentalPriceAsync(int serverId, int rentalDays);
    }
    public class ServerService : IServerService
    {
        private readonly Context _context;
        private readonly ILogger<ServerService> _logger;
        private readonly ICacheService _cacheService;

        public ServerService(
            Context context,
            ILogger<ServerService> logger,
            ICacheService cacheService)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Server> GetServerByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"server_{id}";
                var server = await _cacheService.GetOrCreateAsync(cacheKey, async () =>
                {
                    return await _context.Servers
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == id);
                }, TimeSpan.FromMinutes(30));

                return server;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting server with id {id}");
                throw;
            }
        }

        public async Task<List<Server>> GetAllServersAsync()
        {
            try
            {
                return await _context.Servers
                    .AsNoTracking()
                    .OrderByDescending(s => s.IsPopular)
                    .ThenBy(s => s.DailyPrice)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all servers");
                throw;
            }
        }

        public async Task<List<Server>> GetServersByTypeAsync(ServerType type)
        {
            try
            {
                var cacheKey = $"servers_{type}";
                return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
                {
                    return await _context.Servers
                        .AsNoTracking()
                        .Where(s => s.Type == type)
                        .OrderByDescending(s => s.IsPopular)
                        .ThenBy(s => s.DailyPrice)
                        .ToListAsync();
                }, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting servers by type {type}");
                throw;
            }
        }

        public async Task<List<ServerCategory>> GetServerCategoriesAsync()
        {
            try
            {
                var cacheKey = "server_categories";
                return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
                {
                    var categories = new List<ServerCategory>
                {
                    new ServerCategory
                    {
                        Name = "پلن‌های CPU",
                        Description = "سرورهای پردازشی با قدرت محاسباتی بالا",
                        IconClass = "flaticon-003-network",
                        Servers = await GetServersByTypeAsync(ServerType.CPU)
                    },
                    new ServerCategory
                    {
                        Name = "پلن‌های GPU",
                        Description = "سرورهای پردازش گرافیکی و هوش مصنوعی",
                        IconClass = "flaticon-030-server-1",
                        Servers = await GetServersByTypeAsync(ServerType.GPU)
                    }
                };

                    return categories;
                }, TimeSpan.FromHours(2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting server categories");
                throw;
            }
        }

        public async Task<Server> CreateServerAsync(Server server)
        {
            try
            {
                await _context.Servers.AddAsync(server);
                await _context.SaveChangesAsync();

                // Clear relevant caches
                await _cacheService.RemoveAsync($"servers_{server.Type}");
                await _cacheService.RemoveAsync("server_categories");

                return server;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating server");
                throw;
            }
        }

        public async Task<Server> UpdateServerAsync(Server server)
        {
            try
            {
                var existingServer = await _context.Servers.FindAsync(server.Id);
                if (existingServer == null)
                    throw new Exception("Server not found");

                _context.Entry(existingServer).CurrentValues.SetValues(server);
                existingServer.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Clear relevant caches
                await _cacheService.RemoveAsync($"server_{server.Id}");
                await _cacheService.RemoveAsync($"servers_{server.Type}");
                await _cacheService.RemoveAsync($"servers_{existingServer.Type}"); // In case type changed
                await _cacheService.RemoveAsync("server_categories");

                return existingServer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating server with id {server.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteServerAsync(int id)
        {
            try
            {
                var server = await _context.Servers.FindAsync(id);
                if (server == null)
                    return false;

                _context.Servers.Remove(server);
                await _context.SaveChangesAsync();

                // Clear relevant caches
                await _cacheService.RemoveAsync($"server_{id}");
                await _cacheService.RemoveAsync($"servers_{server.Type}");
                await _cacheService.RemoveAsync("server_categories");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting server with id {id}");
                throw;
            }
        }

        public async Task<List<Server>> GetPopularServersAsync(int count = 4)
        {
            try
            {
                return await _context.Servers
                    .AsNoTracking()
                    .Where(s => s.IsPopular)
                    .OrderBy(s => s.DailyPrice)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular servers");
                throw;
            }
        }

        public async Task<bool> IsServerAvailableAsync(int serverId)
        {
            try
            {
                // اینجا می‌توانید منطق بررسی موجودیت سرور را پیاده‌سازی کنید
                // مثلاً بررسی کنید که چند سرور از این نوع در حال استفاده هستند
                return true; // برای سادگی فعلی همیشه true برمی‌گردانیم
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking availability for server {serverId}");
                throw;
            }
        }

        public async Task<decimal> CalculateRentalPriceAsync(int serverId, int rentalDays)
        {
            try
            {
                var server = await GetServerByIdAsync(serverId);
                if (server == null)
                    throw new Exception("Server not found");

                // محاسبه قیمت با در نظر گرفتن تخفیف‌های احتمالی
                decimal basePrice = server.DailyPrice * rentalDays;

                // در اینجا می‌توانید منطق تخفیف‌ها را اضافه کنید
                // مثلاً تخفیف برای اجاره‌های بلندمدت

                return basePrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating rental price for server {serverId}");
                throw;
            }
        }

    }
}