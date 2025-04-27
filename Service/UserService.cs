
using DataLayer;
using Entity.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Service
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByMobileAsync(long mobile);
        Task<bool> IsMobileUniqueAsync(long mobile, int? excludeId = null);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
        Task<string> GenerateVerificationCodeAsync(long mobile);
        Task<bool> VerifyCodeAsync(long mobile, string code);
        Task<User> LoginAsync(long mobile);
        Task SaveChangesAsync();
    }
    public class UserService : IUserService
    {
        private readonly Context _context;
        private readonly ISmsService _smsService;
        private readonly ICacheService _cacheService;

        public UserService(Context context, ISmsService smsService, ICacheService cacheService)
        {
            _context = context;
            _smsService = smsService;
            _cacheService = cacheService;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByMobileAsync(long mobile)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Mobile == mobile);
        }

        public async Task<bool> IsMobileUniqueAsync(long mobile, int? excludeId = null)
        {
            return !await _context.Users.AnyAsync(u =>
                u.Mobile == mobile &&
                (!excludeId.HasValue || u.Id != excludeId.Value));
        }

        public async Task<User> CreateAsync(User user)
        {
            if (!await IsMobileUniqueAsync(user.Mobile))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            user.CreatedAt = DateTime.Now;
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            if (!await IsMobileUniqueAsync(user.Mobile, user.Id))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
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

            if (user == null || string.IsNullOrWhiteSpace(code)) return false;

            if (!cachedCode.IsNullOrEmpty() && cachedCode == code)
            {
                user.IsMobileVerified = true;
                await UpdateAsync(user);
                return true;
            }

            return false;
        }

        public async Task<User> LoginAsync(long mobile)
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