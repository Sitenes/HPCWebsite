
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Service
{
    public interface IServerRentalService
    {
        Task<ServerRentalOrder> CreateOrderAsync(int billingInformationId, int paymentId, int serverId, int rentalDays);
        Task<ServerRentalOrder> GetOrderByIdAsync(int id);
        Task<List<ServerRentalOrder>> GetUserOrdersAsync(string userId);
    }

    public class ServerRentalService : IServerRentalService
    {
        private readonly Context _context;
        private readonly IServerService _serverService;

        public ServerRentalService(Context context, IServerService serverService)
        {
            _context = context;
            _serverService = serverService;
        }

        public async Task<ServerRentalOrder> CreateOrderAsync(int billingInformationId, int paymentId, int serverId, int rentalDays)
        {
            var server = await _serverService.GetServerByIdAsync(serverId);
            if (server == null)
                throw new Exception("سرور مورد نظر یافت نشد");

            var order = new ServerRentalOrder
            {
                BillingInformationId = billingInformationId,
                PaymentId = paymentId,
                ServerId = serverId,
                ServerName = server.Name,
                ServerSpecs = server.Specifications,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(rentalDays),
                Status = OrderStatus.Pending
            };

            await _context.ServerRentalOrders.AddAsync(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<ServerRentalOrder> GetOrderByIdAsync(int id)
        {
            return await _context.ServerRentalOrders
                .Include(o => o.BillingInformation)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<ServerRentalOrder>> GetUserOrdersAsync(string userId)
        {
            return await _context.ServerRentalOrders
                .Include(o => o.BillingInformation)
                .Include(o => o.Payment)
                .Where(o => o.BillingInformation.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}