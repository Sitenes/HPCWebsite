using Entities.Models.MainEngine;
using Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataLayer.DbContext
{
    public class DynamicDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly IConfiguration _configuration;
        public DbSet<User> User { get; set; }
        public DbSet<HpcUser> HpcUsers { get; set; }
        public DbSet<HpcBillingInformation> HpcBillingInformations { get; set; }
        public DbSet<HpcPayment> HpcPayments { get; set; }
        public DbSet<HpcServer> HpcServers { get; set; }
        public DbSet<HpcTempUserServer> HpcTempUserServers { get; set; }
        public DbSet<HpcServerRentalOrder> HpcServerRentalOrders { get; set; }
        public DbSet<HpcShoppingCart> HpcShoppingCarts { get; set; }
        public DbSet<HpcCartItem> HpcCartItems { get; set; }

        public DynamicDbContext(DbContextOptions<DynamicDbContext> options, IConfiguration configuration) : base(options)
        {
            this._configuration = configuration;
        }
    }
}