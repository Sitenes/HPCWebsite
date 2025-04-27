
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Service
{
    public interface IPaymentService
    {
        Task<Payment> InitiatePaymentAsync(int billingInformationId, decimal amount, PaymentMethod method);
        Task<Payment> VerifyPaymentAsync(string transactionId);
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<Payment> GetPaymentByTransactionIdAsync(string transactionId);

    }

    public class PaymentService : IPaymentService
    {
        private readonly Context _context;
        private readonly IZarinpalService _zarinpalService;
        private readonly IPasargadService _pasargadService;
        private readonly ICacheService _cacheService;

        public PaymentService(
            Context context,
            IZarinpalService zarinpalService,
            IPasargadService pasargadService,
            ICacheService cacheService
            )
        {
            _context = context;
            _zarinpalService = zarinpalService;
            _pasargadService = pasargadService;
            _cacheService = cacheService;
        }

        public async Task<Payment> InitiatePaymentAsync(int billingInformationId, decimal amount, PaymentMethod method)
        {
            var payment = new Payment
            {
                BillingInformationId = billingInformationId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending
            };

            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();

            string transactionId = "";

            switch (method)
            {
                case PaymentMethod.Zarinpal:
                    transactionId = await _zarinpalService.RequestPayment(amount, $"پرداخت برای اجاره سرور - شماره سفارش {payment.Id}");
                    break;
                case PaymentMethod.BankPasargad:
                    transactionId = await _pasargadService.RequestPayment(amount, $"پرداخت برای اجاره سرور - شماره سفارش {payment.Id}");
                    break;
            }

            payment.TransactionId = transactionId;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<Payment> VerifyPaymentAsync(string transactionId)
        {
            var payment = await _context.Payments
                .Include(p => p.BillingInformation)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

            if (payment == null)
                throw new Exception("پرداخت یافت نشد");

            bool isVerified = false;

            switch (payment.Method)
            {
                case PaymentMethod.Zarinpal:
                    isVerified = await _zarinpalService.VerifyPayment(transactionId, payment.Amount);
                    break;
                case PaymentMethod.BankPasargad:
                    isVerified = await _pasargadService.VerifyPayment(transactionId, payment.Amount);
                    break;
            }

            payment.Status = isVerified ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.BillingInformation)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Payment> GetPaymentByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                throw new ArgumentNullException(nameof(transactionId), "شناسه تراکنش نمی‌تواند خالی باشد");
            }

                var cacheKey = $"payment_txn_{transactionId}";

                var payment = await _cacheService.GetOrCreateAsync(cacheKey, async () =>
                {
                    var paymentFromDb = await _context.Payments
                        .AsNoTracking()
                        .Include(p => p.BillingInformation)
                        .Include(p => p.ServerRentalOrder)
                        .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                    if (paymentFromDb == null)
                    {
                        throw new KeyNotFoundException($"تراکنش با شناسه {transactionId} یافت نشد");
                    }

                    return paymentFromDb;
                }, TimeSpan.FromMinutes(15));

                return payment;
            }
    }
}