
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using DataLayer.DbContext;
using Entities.Models.MainEngine;
using System.Security.Claims;

namespace Service
{
    public interface IBillingService
    {
        Task<HpcBillingInformation?> GetUserBillingInformationAsync(int userId);
        Task<HpcBillingInformation> SaveBillingInformationAsync(HpcBillingInformation model, int userId);
        Task<bool> HasBillingInformationAsync(int userId);
    }

    public class BillingService : IBillingService
    {
        private readonly DynamicDbContext _context;
        private readonly Context _basicContext;

        public BillingService(DynamicDbContext context,Context basicContext)
        {
            _context = context;
            this._basicContext = basicContext;
        }

        public async Task<HpcBillingInformation?> GetUserBillingInformationAsync(int userId)
        {
            return await _context.HpcBillingInformations
                .FirstOrDefaultAsync(b => b.UserId == userId);
        }

        public async Task<HpcBillingInformation> SaveBillingInformationAsync(HpcBillingInformation model, int dashboardUserId)
        {
            var existingInfo = await GetUserBillingInformationAsync(model.UserId);

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

                _context.HpcBillingInformations.Update(existingInfo);
                await _context.SaveChangesAsync();
                return existingInfo;
            }
            else
            {
                var workflowUser = new Entities.Models.Workflows.Workflow_User { WorkflowId = 1, UserId = dashboardUserId };
                await _basicContext.Workflow_User.AddAsync(workflowUser);
                await _basicContext.SaveChangesAsync();
                model.WorkflowUserId = workflowUser.Id;

                await _context.HpcBillingInformations.AddAsync(model);

                await _context.SaveChangesAsync();
                return model;
            }
        }

        public async Task<bool> HasBillingInformationAsync(int userId)
        {
            return await _context.HpcBillingInformations
                .AnyAsync(b => b.UserId == userId);
        }
    }
}