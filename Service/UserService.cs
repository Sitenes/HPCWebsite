
using DataLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Entity;
using DataLayer.DbContext;

namespace Service
{
    public interface IUserService
    {
        Task<HpcUser?> GetByIdAsync(int id);
        Task<HpcUser?> GetByMobileAsync(long mobile);
        Task<bool> IsMobileUniqueAsync(long mobile, int? excludeId = null);
        Task<HpcUser> CreateAsync(HpcUser user, int dashboardUserId);
        Task<HpcUser> UpdateAsync(HpcUser user);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerateVerificationCodeAsync(long mobile);
        Task<bool> VerifyCodeAsync(long mobile, string code);
        Task<HpcUser> LoginAsync(long mobile);
        Task<HpcUser?> GetByDashboardUserIdAsync(int dashboardUserId);
        Task SaveChangesAsync();
    }
    public class UserService : IUserService
    {
        private readonly DynamicDbContext _context;
        private readonly ISmsService _smsService;
        private readonly ICacheService _cacheService;
        private readonly Context _basicContext;

        public UserService(DynamicDbContext context, ISmsService smsService, ICacheService cacheService,Context basicContext)
        {
            _context = context;
            _smsService = smsService;
            _cacheService = cacheService;
            this._basicContext = basicContext;
        }

        public async Task<HpcUser?> GetByIdAsync(int id)
        {
            return await _context.HpcUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<HpcUser?> GetByDashboardUserIdAsync(int dashboardUserId)
        {
            return await _context.HpcUsers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == dashboardUserId);
        }
        public async Task<HpcUser?> GetByMobileAsync(long mobile)
        {
            return await _context.HpcUsers.FirstOrDefaultAsync(u => u.Mobile == mobile);
        }

        public async Task<bool> IsMobileUniqueAsync(long mobile, int? excludeId = null)
        {
            return !await _context.HpcUsers.AnyAsync(u =>
                u.Mobile == mobile &&
                (!excludeId.HasValue || u.Id != excludeId.Value));
        }

        public async Task<HpcUser> CreateAsync(HpcUser user, int dashboardUserId)
        {
            if (!await IsMobileUniqueAsync(user.Mobile) || await _context.User.AnyAsync(x => x.UserName == user.Mobile.ToString()))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            var workflowUser = new Entities.Models.Workflows.Workflow_User { WorkflowId = 1, UserId = dashboardUserId };
            await _basicContext.Workflow_User.AddAsync(workflowUser);
            await _context.SaveChangesAsync();
            user.WorkflowUserId = workflowUser.Id;

            user.CreatedAt = DateTime.Now;
            var dashboardUser = new Entities.Models.MainEngine.User
            {
                Name = user.FirstName + " " + user.LastName,
                UserName = user.Mobile.ToString()
            };
            await _context.User.AddAsync(dashboardUser);
            await _context.SaveChangesAsync();
            user.UserId = dashboardUser.Id;
            await _context.HpcUsers.AddAsync(user);
           
           
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<HpcUser> UpdateAsync(HpcUser user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            var dashboardUser = await _context.User.FirstOrDefaultAsync(x=>x.Id == user.UserId);
            dashboardUser.Name = user.FirstName + " " + user.LastName;
            _context.HpcUsers.Update(user);
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            _context.HpcUsers.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<string> GenerateVerificationCodeAsync(long mobile)
        {
            var user = await GetByMobileAsync(mobile);
            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد");

            var code = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(5);

            await _cacheService.SetAsync($"verification_{mobile}", code, TimeSpan.FromMinutes(5));
            await _smsService.SendVerificationCodeAsync(mobile, code);

            await UpdateAsync(user);

            return code;
        }

        public async Task<bool> VerifyCodeAsync(long mobile, string code)
        {
            var cachedCode = await _cacheService.GetAsync<string>($"verification_{mobile}");
            var user = await GetByMobileAsync(mobile);

            //if (user == null || string.IsNullOrWhiteSpace(code)) return false; // TODO : change too validate

            //if (!cachedCode.IsNullOrEmpty() && cachedCode == code)
            //{
                user.IsMobileVerified = true;
                await UpdateAsync(user);
                return true;
            //}

            return false;
        }

        public async Task<HpcUser> LoginAsync(long mobile)
        {
            var user = await GetByMobileAsync(mobile);
            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("کاربر یافت نشد یا غیرفعال است");

            if (!user.IsMobileVerified)
                throw new UnauthorizedAccessException("شماره موبایل تایید نشده است");

            return user;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}