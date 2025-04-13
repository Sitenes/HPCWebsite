using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http.Json;
using ViewModel;

namespace Service
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string mobileNumber, string message);
        Task<bool> SendVerificationCodeAsync(string mobileNumber, string code);
    }

    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly SmsConfiguration _config;

        public SmsService(HttpClient httpClient, SmsConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<bool> SendSmsAsync(string mobileNumber, string message)
        {
            try
            {
                // ساخت درخواست برای ارسال SMS
                var request = new SmsRequest
                {
                    ReceiverNumber = mobileNumber,
                    Message = message,
                    ApiKey = _config.ApiKey,
                    SenderNumber = _config.SenderNumber
                };

                // ارسال درخواست به سرویس SMS
                var response = await _httpClient.PostAsJsonAsync(_config.ApiUrl, request);

                // بررسی موفقیت آمیز بودن ارسال
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string mobileNumber, string code)
        {
            // ساخت متن پیام تایید
            var message = $"کد تایید شما: {code}\nاین کد تا 5 دقیقه معتبر است.";

            // ارسال پیام
            return await SendSmsAsync(mobileNumber, message);
        }
    }
}