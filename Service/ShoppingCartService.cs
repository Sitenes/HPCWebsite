
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using DataLayer.DbContext;

namespace Service
{
    public interface IShoppingCartService
    {
        Task<HpcShoppingCart> GetUserCartAsync(int userId);
        Task AddToCartAsync(int userId, int serverId, int rentalDays, int dashboardUserId);
        Task RemoveFromCartAsync(int userId, int itemId);
        Task UpdateCartItemAsync(int userId, int itemId, int rentalDays);
        Task ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly DynamicDbContext _context;
        private readonly IServerService _serverService;
        private readonly ILogger<ShoppingCartService> _logger;
        private readonly Context _basicContext;

        public ShoppingCartService(
            DynamicDbContext context,
            IServerService serverService,
            ILogger<ShoppingCartService> logger,
            Context basicContext)
        {
            _context = context;
            _serverService = serverService;
            _logger = logger;
            this._basicContext = basicContext;
        }

        public async Task<HpcShoppingCart> GetUserCartAsync(int userId)
        {
            var cart = await _context.HpcShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new HpcShoppingCart { UserId = userId };

                await _context.HpcShoppingCarts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            // محاسبه تخفیف (مثال: 5% تخفیف برای بیش از 2 آیتم)
            if (cart.Items.Count > 2)
            {
                cart.DiscountAmount = cart.SubTotal * 0.05m;
            }

            return cart;
        }

        public async Task AddToCartAsync(int userId, int serverId, int rentalDays, int dashboardUserId)
        {
            var server = await _serverService.GetServerByIdAsync(serverId);
            if (server == null)
            {
                throw new Exception("سرور مورد نظر یافت نشد");
            }

            var cart = await GetUserCartAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ServerId == serverId);
            if (existingItem != null)
            {
                existingItem.RentalDays += rentalDays;
            }
            else
            {
                var workflowUser = new Entities.Models.Workflows.Workflow_User { WorkflowId = 1, UserId = dashboardUserId };
                await _basicContext.Workflow_User.AddAsync(workflowUser);
                await _context.SaveChangesAsync();
                var wkID = workflowUser.Id;

                cart.Items.Add(new HpcCartItem
                {
                    ServerId = serverId,
                    ServerName = server.Name,
                    ServerSpecs = server.Specifications,
                    ImageUrl = $"/images/servers/{serverId}.jpg",
                    DailyPrice = server.DailyPrice,
                    RentalDays = rentalDays,
                    WorkflowUserId = wkID
                });
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(int userId, int itemId)
        {
            var cart = await GetUserCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (item != null)
            {
                _context.HpcCartItems.Remove(item);
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCartItemAsync(int userId, int itemId, int rentalDays)
        {
            var cart = await GetUserCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);

            if (item != null)
            {
                item.RentalDays = rentalDays;
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await GetUserCartAsync(userId);
            _context.HpcCartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartItemCountAsync(int userId)
        {
            var cart = await GetUserCartAsync(userId);
            return cart.Items.Count;
        }
    }
}