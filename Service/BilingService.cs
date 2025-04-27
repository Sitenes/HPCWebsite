
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Service
{
    public interface IBillingService
    {
        Task<BillingInformation?> GetUserBillingInformationAsync(string userId);
        Task<BillingInformation> SaveBillingInformationAsync(BillingInformation model, string userId);
        Task<bool> HasBillingInformationAsync(string userId);
    }

    public class BillingService : IBillingService
    {
        private readonly Context _context;

        public BillingService(Context context)
        {
            _context = context;
        }

        public async Task<BillingInformation?> GetUserBillingInformationAsync(string userId)
        {
            return await _context.BillingInformations
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task<BillingInformation> SaveBillingInformationAsync(BillingInformation model, string userId)
        {
            var existingInfo = await GetUserBillingInformationAsync(userId);

            if (existingInfo != null)
            {
                // Update existing
                existingInfo.FirstName = model.FirstName;
                existingInfo.LastName = model.LastName;
                existingInfo.Email = model.Email;
                existingInfo.PhoneNumber = model.PhoneNumber;
                existingInfo.CompanyName = model.CompanyName;
                existingInfo.Website = model.Website;
                existingInfo.FullAddress = model.FullAddress;
                existingInfo.Country = model.Country;
                existingInfo.Province = model.Province;
                existingInfo.PostalCode = model.PostalCode;
                existingInfo.RentalDays = model.RentalDays;
                existingInfo.UpdatedAt = DateTime.Now;

                _context.BillingInformations.Update(existingInfo);
                await _context.SaveChangesAsync();
                return existingInfo;
            }
            else
            {
                // Create new
                model.UserId = userId;
                await _context.BillingInformations.AddAsync(model);
                await _context.SaveChangesAsync();
                return model;
            }
        }

        public async Task<bool> HasBillingInformationAsync(string userId)
        {
            return await _context.BillingInformations
                .AnyAsync(b => b.UserId == userId);
        }
    }
}