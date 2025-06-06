
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using DataLayer.DbContext;

namespace Service
{
    public interface IServerRentalService
    {
        Task<HpcServerRentalOrder> CreateOrderAsync(int paymentId, int serverId, int rentalDays, int dashboardUserId);
        Task<HpcServerRentalOrder> GetOrderByIdAsync(int id);
        Task<List<HpcServerRentalOrder>> GetUserOrdersAsync(int userId);
        Task<List<HpcServer>> GetUserServersAsync(int userId);
    }

    public class ServerRentalService : IServerRentalService
    {
        private readonly DynamicDbContext _context;
        private readonly IServerService _serverService;
        private readonly Context _basicContext;

        public ServerRentalService(DynamicDbContext context, IServerService serverService,Context basicContext)
        {
            _context = context;
            _serverService = serverService;
            this._basicContext = basicContext;
        }

        public async Task<HpcServerRentalOrder> GetOrderByIdAsync(int id)
        {
            return await _context.HpcServerRentalOrders
                .Include(o => o.Server)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<HpcServerRentalOrder>> GetUserOrdersAsync(int userId)
        {
            return await _context.HpcServerRentalOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<HpcServer>> GetUserServersAsync(int userId)
        {
            var activeOrders = await _context.HpcServerRentalOrders
                .Where(o => o.UserId == userId /* && o.Status == OrderStatus.Active */)
                .Include(o => o.Server)
                .ToListAsync();

            return activeOrders.Select(o => o.Server).ToList();
        }
        public async Task<HpcServerRentalOrder> CreateOrderAsync(int userId, int serverId, int rentalDays, int dashboardUserId)
        {
            var server = await _serverService.GetServerByIdAsync(serverId);
            if (server == null)
                throw new Exception("سرور مورد نظر یافت نشد");

            var order = new HpcServerRentalOrder
            {
                UserId = userId,
                ServerId = serverId,
                ServerName = server.Name,
                ServerSpecs = server.Specifications,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(rentalDays),
                Status = OrderStatus.Pending
            };
            var workflowUser = new Entities.Models.Workflows.Workflow_User { WorkflowId = 1, UserId = dashboardUserId };
            await _basicContext.Workflow_User.AddAsync(workflowUser);
            await _context.SaveChangesAsync();
            order.WorkflowUserId = workflowUser.Id;

            await _context.HpcServerRentalOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            return order;
        }

    }
}