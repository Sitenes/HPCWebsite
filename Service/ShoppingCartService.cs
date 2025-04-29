
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
    public interface IShoppingCartService
    {
        Task<ShoppingCart> GetUserCartAsync(int userId);
        Task AddToCartAsync(int userId, int serverId, int rentalDays);
        Task RemoveFromCartAsync(int userId, int itemId);
        Task UpdateCartItemAsync(int userId, int itemId, int rentalDays);
        Task ClearCartAsync(int userId);
        Task<int> GetCartItemCountAsync(int userId);
    }

    public class ShoppingCartService : IShoppingCartService
    {
        private readonly Context _context;
        private readonly IServerService _serverService;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(
            Context context,
            IServerService serverService,
            ILogger<ShoppingCartService> logger)
        {
            _context = context;
            _serverService = serverService;
            _logger = logger;
        }

        public async Task<ShoppingCart> GetUserCartAsync(int userId)
        {
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId };
                await _context.ShoppingCarts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            // محاسبه تخفیف (مثال: 5% تخفیف برای بیش از 2 آیتم)
            if (cart.Items.Count > 2)
            {
                cart.DiscountAmount = cart.SubTotal * 0.05m;
            }

            return cart;
        }

        public async Task AddToCartAsync(int userId, int serverId, int rentalDays)
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
                cart.Items.Add(new CartItem
                {
                    ServerId = serverId,
                    ServerName = server.Name,
                    ServerSpecs = server.Specifications,
                    ImageUrl = $"/images/servers/{serverId}.jpg",
                    DailyPrice = server.DailyPrice,
                    RentalDays = rentalDays
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
                _context.CartItems.Remove(item);
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
            _context.CartItems.RemoveRange(cart.Items);
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