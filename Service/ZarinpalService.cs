
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Service
{
    public interface IZarinpalService
    {
        Task<string> RequestPayment(decimal amount, string description);
        Task<bool> VerifyPayment(string authority, decimal amount);
        Task<ZarinpalPaymentData> GetPaymentData(string authority);
    }

    public class ZarinpalService : IZarinpalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZarinpalService> _logger;

        public ZarinpalService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ZarinpalService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // تنظیمات پایه برای HttpClient
            _httpClient.BaseAddress = new Uri(_configuration["Zarinpal:BaseUrl"]);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> RequestPayment(decimal amount, string description)
        {
            try
            {
                var requestData = new
                {
                    MerchantID = _configuration["Zarinpal:MerchantId"],
                    Amount = amount * 10, // تبدیل به ریال
                    Description = description,
                    CallbackURL = _configuration["Zarinpal:CallbackUrl"],
                    Metadata = new { Mobile = "", Email = "" }
                };

                var response = await _httpClient.PostAsJsonAsync("payment/request.json", requestData);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<ZarinpalResponse>();

                if (responseData.Status != 100)
                {
                    _logger.LogError($"Zarinpal payment request failed with status: {responseData.Status}");
                    throw new Exception($"خطا در ارتباط با درگاه زرین‌پال. کد خطا: {responseData.Status}");
                }

                return responseData.Authority;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Zarinpal payment request");
                throw new Exception("خطا در ارتباط با درگاه پرداخت زرین‌پال");
            }
        }

        public async Task<bool> VerifyPayment(string authority, decimal amount)
        {
            try
            {
                var requestData = new
                {
                    MerchantID = _configuration["Zarinpal:MerchantId"],
                    Amount = amount * 10, // تبدیل به ریال
                    Authority = authority
                };

                var response = await _httpClient.PostAsJsonAsync("payment/verify.json", requestData);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<ZarinpalVerifyResponse>();

                if (responseData.Status != 100 && responseData.Status != 101)
                {
                    _logger.LogError($"Zarinpal payment verification failed with status: {responseData.Status}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Zarinpal payment verification");
                return false;
            }
        }

        public async Task<ZarinpalPaymentData> GetPaymentData(string authority)
        {
            try
            {
                var requestData = new
                {
                    MerchantID = _configuration["Zarinpal:MerchantId"],
                    Authority = authority
                };

                var response = await _httpClient.PostAsJsonAsync("payment/verify.json", requestData);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<ZarinpalVerifyResponse>();

                return new ZarinpalPaymentData
                {
                    Authority = authority,
                    Status = responseData.Status,
                    RefId = responseData.RefID.ToString(),
                    CardPan = responseData.CardPan,
                    CardHash = responseData.CardHash,
                    FeeType = responseData.FeeType,
                    Fee = responseData.Fee
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Zarinpal payment data");
                throw;
            }
        }

        private class ZarinpalResponse
        {
            public int Status { get; set; }
            public string Authority { get; set; }
        }

        private class ZarinpalVerifyResponse
        {
            public int Status { get; set; }
            public long RefID { get; set; }
            public string CardPan { get; set; }
            public string CardHash { get; set; }
            public string FeeType { get; set; }
            public decimal Fee { get; set; }
        }
    }
}