
using DataLayer;
using Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using DataLayer.DbContext;

namespace Service
{
    public interface IPaymentService
    {
        Task<HpcPayment> InitiatePaymentAsync(int billingInformationId, string transactionId, decimal amount, PaymentMethod method, int dashboardUserId);
        Task<HpcPayment> VerifyPaymentAsync(string transactionId);
        Task<HpcPayment?> GetPaymentByIdAsync(int id);
        Task<HpcPayment> GetPaymentByTransactionIdAsync(string transactionId);

    }
    public class PaymentService : IPaymentService
    {
        private readonly DynamicDbContext _context;
        private readonly IZarinpalService _zarinpalService;
        private readonly IPasargadService _pasargadService;
        private readonly ICacheService _cacheService;
        private readonly Context _basicContext;

        public PaymentService(
            DynamicDbContext context,
            IZarinpalService zarinpalService,
            IPasargadService pasargadService,
            ICacheService cacheService,
            Context basicContext)
        {
            _context = context;
            _zarinpalService = zarinpalService;
            _pasargadService = pasargadService;
            _cacheService = cacheService;
            this._basicContext = basicContext;
        }

        public async Task<HpcPayment> InitiatePaymentAsync(int shoppingCartId, string transactionId, decimal amount, PaymentMethod method,int dashboardUserId)
        {
            var payment = new HpcPayment
            {
                ShoppingCartId = shoppingCartId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.Now,
                TransactionId = transactionId
            };
            var workflowUser = new Entities.Models.Workflows.Workflow_User { WorkflowId = 1, UserId = dashboardUserId };
            await _basicContext.Workflow_User.AddAsync(workflowUser);
            await _context.SaveChangesAsync();
            payment.WorkflowUserId = workflowUser.Id;

            await _context.HpcPayments.AddAsync(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<HpcPayment> VerifyPaymentAsync(string transactionId)
        {
            var payment = await _context.HpcPayments
                .Include(p => p.ShoppingCart.Items)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

            if (payment == null)
                throw new Exception("پرداخت یافت نشد");

            //bool isVerified = payment.Method switch
            //{
            //    PaymentMethod.Zarinpal => await _zarinpalService.VerifyPayment(transactionId, payment.Amount),
            //    PaymentMethod.BankPasargad => await _pasargadService.VerifyPayment(transactionId, payment.Amount),
            //    _ => false
            //};
            bool isVerified = true;

            payment.Status = isVerified ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.PaymentDate = DateTime.Now;
            payment.UpdatedAt = DateTime.Now;

            _context.HpcPayments.Update(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<HpcPayment?> GetPaymentByIdAsync(int id)
        {
            return await _context.HpcPayments
                .Include(p => p.ShoppingCart)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<HpcPayment> GetPaymentByTransactionIdAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentNullException(nameof(transactionId), "شناسه تراکنش نمی‌تواند خالی باشد");

            var cacheKey = $"payment_txn_{transactionId}";

            var payment = await _cacheService.GetOrCreateAsync(cacheKey, async () =>
            {
                var paymentFromDb = await _context.HpcPayments
                    .AsNoTracking()
                    .Include(p => p.ShoppingCart)
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                if (paymentFromDb == null)
                    throw new KeyNotFoundException($"تراکنش با شناسه {transactionId} یافت نشد");

                return paymentFromDb;
            }, TimeSpan.FromMinutes(15));

            return payment;
        }
    }

}