
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
using System.Text;
using System.Security.Cryptography;

namespace Service
{
    public interface IPasargadService
    {
        Task<string> RequestPayment(decimal amount, string description);
        Task<bool> VerifyPayment(string referenceId, decimal amount);
        Task<PasargadPaymentData> GetPaymentData(string referenceId);
    }
    public class PasargadService : IPasargadService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PasargadService> _logger;

        public PasargadService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PasargadService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // تنظیمات پایه برای HttpClient
            _httpClient.BaseAddress = new Uri(_configuration["Pasargad:BaseUrl"]);
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
                    MerchantCode = _configuration["Pasargad:MerchantCode"],
                    TerminalCode = _configuration["Pasargad:TerminalCode"],
                    Amount = amount * 10, // تبدیل به ریال
                    RedirectAddress = _configuration["Pasargad:CallbackUrl"],
                    InvoiceNumber = GenerateInvoiceNumber(),
                    InvoiceDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Action = 1003, // برای پرداخت
                    TimeStamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Mobile = "",
                    Email = "",
                    Description = description
                };

                // محاسبه signature
                var signData = $"{requestData.TerminalCode};{requestData.InvoiceNumber};{requestData.InvoiceDate};" +
                               $"{requestData.Amount};{requestData.RedirectAddress};{requestData.Action};" +
                               $"{requestData.TimeStamp}";

                requestData.GetType().GetProperty("Sign")?
                    .SetValue(requestData, CalculateSignature(signData));

                var response = await _httpClient.PostAsJsonAsync("api/v1/Payment/Request", requestData);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<PasargadResponse>();

                if (responseData.Status != 0)
                {
                    _logger.LogError($"Pasargad payment request failed with status: {responseData.Status}");
                    throw new Exception($"خطا در ارتباط با درگاه پاسارگاد. کد خطا: {responseData.Status}");
                }

                return responseData.ReferenceId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Pasargad payment request");
                throw new Exception("خطا در ارتباط با درگاه پرداخت پاسارگاد");
            }
        }

        public async Task<bool> VerifyPayment(string referenceId, decimal amount)
        {
            try
            {
                var requestData = new
                {
                    MerchantCode = _configuration["Pasargad:MerchantCode"],
                    TerminalCode = _configuration["Pasargad:TerminalCode"],
                    InvoiceNumber = GetInvoiceNumberFromReference(referenceId),
                    InvoiceDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Amount = amount * 10 // تبدیل به ریال
                };

                // محاسبه signature
                var signData = $"{requestData.TerminalCode};{requestData.InvoiceNumber};{requestData.InvoiceDate};" +
                               $"{requestData.Amount}";

                requestData.GetType().GetProperty("Sign")?
                    .SetValue(requestData, CalculateSignature(signData));

                var response = await _httpClient.PostAsJsonAsync("api/v1/Payment/Verify", requestData);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<PasargadVerifyResponse>();

                if (responseData.IsSuccess)
                {
                    return true;
                }

                _logger.LogError($"Pasargad payment verification failed. Status: {responseData.Status}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Pasargad payment verification");
                return false;
            }
        }

        public async Task<PasargadPaymentData> GetPaymentData(string referenceId)
        {
            try
            {
                var requestData = new
                {
                    MerchantCode = _configuration["Pasargad:MerchantCode"],
                    TerminalCode = _configuration["Pasargad:TerminalCode"],
                    InvoiceNumber = GetInvoiceNumberFromReference(referenceId),
                    InvoiceDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                };

                // محاسبه signature
                var signData = $"{requestData.TerminalCode};{requestData.InvoiceNumber};{requestData.InvoiceDate}";

                requestData.GetType().GetProperty("Sign")?
                    .SetValue(requestData, CalculateSignature(signData));

                var response = await _httpClient.PostAsJsonAsync("api/v1/Payment/Check", requestData);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<PasargadCheckResponse>();

                return new PasargadPaymentData
                {
                    ReferenceId = referenceId,
                    TransactionDate = responseData.TransactionDate,
                    MaskedCardNumber = responseData.MaskedCardNumber,
                    InvoiceNumber = responseData.InvoiceNumber,
                    Status = responseData.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Pasargad payment data");
                throw;
            }
        }

        private string GenerateInvoiceNumber()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16);
        }

        private string GetInvoiceNumberFromReference(string referenceId)
        {
            // در اینجا باید منطق استخراج InvoiceNumber از ReferenceId پیاده‌سازی شود
            // برای سادگی، فرض می‌کنیم ReferenceId همان InvoiceNumber است
            return referenceId;
        }

        private string CalculateSignature(string data)
        {
            // محاسبه signature با استفاده از کلید خصوصی
            // این یک پیاده‌سازی ساده است و در محیط تولید باید با دقت بیشتری پیاده‌سازی شود
            var privateKey = _configuration["Pasargad:PrivateKey"];
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(data + privateKey);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private class PasargadResponse
        {
            public int Status { get; set; }
            public string ReferenceId { get; set; }
        }

        private class PasargadVerifyResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }
            public int Status { get; set; }
        }

        private class PasargadCheckResponse
        {
            public int Status { get; set; }
            public string TransactionDate { get; set; }
            public string MaskedCardNumber { get; set; }
            public string InvoiceNumber { get; set; }
        }
    }
}