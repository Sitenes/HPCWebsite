
using DataLayer;
using Entity.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Service
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByMobileAsync(string mobile);
        Task<bool> IsMobileUniqueAsync(string mobile, Guid? excludeId = null);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly Context _context;

        public UserRepository(Context context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByMobileAsync(string mobile)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Mobile == mobile);
        }

        public async Task<bool> IsMobileUniqueAsync(string mobile, Guid? excludeId = null)
        {
            return !await _context.Users.AnyAsync(u => u.Mobile == mobile && (!excludeId.HasValue || u.Id != excludeId.Value));
        }

        public async Task<User> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }

    public interface IUserService
    {
        Task<User> GetByIdAsync(Guid id);
        Task<User> GetByMobileAsync(string mobile);
        Task<bool> IsMobileUniqueAsync(string mobile, Guid? excludeId = null);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<string> GenerateVerificationCodeAsync(string mobile);
        Task<bool> VerifyCodeAsync(string mobile, string code);
        Task<User> LoginAsync(string mobile);
        Task<bool> ResetPasswordAsync(string mobile, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISmsService _smsService;

        public UserService(
            IUserRepository userRepository,
            ISmsService smsService)
        {
            _userRepository = userRepository;
            _smsService = smsService;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetByMobileAsync(string mobile)
        {
            return await _userRepository.GetByMobileAsync(mobile);
        }

        public async Task<bool> IsMobileUniqueAsync(string mobile, Guid? excludeId = null)
        {
            return await _userRepository.IsMobileUniqueAsync(mobile, excludeId);
        }

        public async Task<User> CreateAsync(User user)
        {
            if (!await IsMobileUniqueAsync(user.Mobile))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            return await _userRepository.AddAsync(user);
        }

        public async Task<User> UpdateAsync(User user)
        {
            if (!await IsMobileUniqueAsync(user.Mobile, user.Id))
                throw new InvalidOperationException("شماره موبایل تکراری است");

            user.UpdatedAt = DateTime.Now;
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user == null) return false;

            return await _userRepository.DeleteAsync(user);
        }

        public async Task<string> GenerateVerificationCodeAsync(string mobile)
        {
            var user = await GetByMobileAsync(mobile);
            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد");

            var code = new Random().Next(10000, 99999).ToString();
            var expiry = DateTime.Now.AddMinutes(5);

            await _cacheService.SetAsync($"verification_{mobile}", code, TimeSpan.FromMinutes(5));
            await _smsService.SendVerificationCodeAsync(mobile, code);

            user.VerificationCode = code;
            user.VerificationCodeExpiry = expiry;
            await _userRepository.UpdateAsync(user);

            return code;
        }

        public async Task<bool> VerifyCodeAsync(string mobile, string code)
        {
            var cachedCode = await _cacheService.GetAsync<string>($"verification_{mobile}");
            User user = null;
            if (cachedCode == code)
            {
                user = await GetByMobileAsync(mobile);
                user.IsMobileVerified = true;
                await _userRepository.UpdateAsync(user);
                return true;
            }
            user = await GetByMobileAsync(mobile);
            if (user == null || user.VerificationCode != code || user.VerificationCodeExpiry < DateTime.Now)
                return false;

            user.IsMobileVerified = true;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<User> LoginAsync(string mobile)
        {
            var user = await GetByMobileAsync(mobile);
            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("کاربر یافت نشد یا غیرفعال است");

            if (!user.IsMobileVerified)
                throw new UnauthorizedAccessException("شماره موبایل تایید نشده است");

            return user;
        }

        public async Task<bool> ResetPasswordAsync(string mobile, string newPassword)
        {
            var user = await GetByMobileAsync(mobile);
            if (user == null) return false;

            // TODO: Implement password hashing
            user.UpdatedAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}