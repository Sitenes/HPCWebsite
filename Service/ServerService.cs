using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using DataLayer.DbContext;

namespace Service
{
    public interface IServerService
    {
        Task<HpcServer> GetServerByIdAsync(int id);
        Task<List<HpcServer>> GetAllServersAsync();
        Task<List<HpcServer>> GetServersByTypeAsync(ServerType type);
        Task<HpcServer> CreateServerAsync(HpcServer server, int dashboardUserId);
        Task<HpcServer> UpdateServerAsync(HpcServer server);
        Task<bool> DeleteServerAsync(int id);
        Task<List<HpcServer>> GetPopularServersAsync(int count = 4);
        Task<bool> IsServerAvailableAsync(int serverId);
        Task<decimal> CalculateRentalPriceAsync(int serverId, int rentalDays);
    }

    public class ServerService : IServerService
    {
        private readonly DynamicDbContext _context;
        private readonly ILogger<ServerService> _logger;
        private readonly ICacheService _cacheService;
        private readonly Context _basicContext;

        public ServerService(
            DynamicDbContext context,
            ILogger<ServerService> logger,
            ICacheService cacheService,
            Context basicContext)
        {
            _context = context;
            _logger = logger;
            _cacheService = cacheService;
            this._basicContext = basicContext;
        }

        public async Task<HpcServer> GetServerByIdAsync(int id)
        {
            try
            {
                return await _context.HpcServers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting server with id {id}");
                throw;
            }
        }

        public async Task<List<HpcServer>> GetAllServersAsync()
        {
            try
            {
                return await _context.HpcServers
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

        public async Task<List<HpcServer>> GetServersByTypeAsync(ServerType type)
        {
            try
            {
              
                    return await _context.HpcServers
                        .AsNoTracking()
                        .Where(s => s.Type == type)
                        .OrderBy(s => s.DailyPrice)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting servers by type {type}");
                throw;
            }
        }

        public async Task<HpcServer> CreateServerAsync(HpcServer server, int dashboardUserId)
        {
            try
            {
                var workflowUser = new Entities.Models.Workflows.Workflow_User
                {
                    WorkflowId = 1,
                    UserId = dashboardUserId
                };

                await _basicContext.Workflow_User.AddAsync(workflowUser);
                await _basicContext.SaveChangesAsync();
                server.WorkflowUserId = workflowUser.Id;

                await _context.HpcServers.AddAsync(server);
                await _context.SaveChangesAsync();

                await _cacheService.RemoveAsync($"servers_{server.Type}");
                await _cacheService.RemoveAsync($"server_{server.Id}");

                return server;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating server");
                throw;
            }
        }

        public async Task<HpcServer> UpdateServerAsync(HpcServer server)
        {
            try
            {
                var existingServer = await _context.HpcServers.FindAsync(server.Id);
                if (existingServer == null)
                    throw new Exception("Server not found");

                _context.Entry(existingServer).CurrentValues.SetValues(server);
                existingServer.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                await _cacheService.RemoveAsync($"server_{server.Id}");
                await _cacheService.RemoveAsync($"servers_{server.Type}");
                await _cacheService.RemoveAsync($"servers_{existingServer.Type}");

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
                var server = await _context.HpcServers.FindAsync(id);
                if (server == null)
                    return false;

                _context.HpcServers.Remove(server);
                await _context.SaveChangesAsync();

                await _cacheService.RemoveAsync($"server_{id}");
                await _cacheService.RemoveAsync($"servers_{server.Type}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting server with id {id}");
                throw;
            }
        }

        public async Task<List<HpcServer>> GetPopularServersAsync(int count = 4)
        {
            try
            {
                return await _context.HpcServers
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
                return true;
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

                decimal basePrice = server.DailyPrice * rentalDays;

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
