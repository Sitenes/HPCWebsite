using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service;
using ViewModel;

namespace HPCWebsite.Controllers
{
    [ValidateAntiForgeryToken]
    [Authorize]

    public class DashboardController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IPaymentService _paymentService;
        private readonly IServerRentalService _serverRentalService;
        private readonly UserManager<User> _userManager;

        public DashboardController(
            IBillingService billingService,
            IPaymentService paymentService,
            IServerRentalService serverRentalService,
            UserManager<User> userManager)
        {
            _billingService = billingService;
            _paymentService = paymentService;
            _serverRentalService = serverRentalService;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ServerList()
        {
            return View();
        }
        public IActionResult ServerInfo()
        {
            return View();
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var billingInfo = await _billingService.GetUserBillingInformationAsync(userId);

            var model = billingInfo ?? new BillingInformation();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(BillingInformation model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var billingInfo = await _billingService.SaveBillingInformationAsync(model, userId);

            // در اینجا می‌توانید کاربر را به صفحه پرداخت هدایت کنید
            return RedirectToAction("Payment", new { billingId = billingInfo.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int billingId)
        {
            var userId = _userManager.GetUserId(User);
            var billingInfo = await _billingService.GetUserBillingInformationAsync(userId);

            if (billingInfo == null || billingInfo.Id != billingId)
            {
                return RedirectToAction("Index");
            }

            // محاسبه مبلغ بر اساس مدت اجاره و نوع سرور
            decimal amount = CalculateRentalAmount(billingInfo.RentalDays);

            ViewBag.BillingId = billingId;
            ViewBag.Amount = amount;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InitiatePayment(int billingId, PaymentMethod method)
        {
            var userId = _userManager.GetUserId(User);
            var billingInfo = await _billingService.GetUserBillingInformationAsync(userId);

            if (billingInfo == null || billingInfo.Id != billingId)
            {
                return RedirectToAction("Index");
            }

            decimal amount = CalculateRentalAmount(billingInfo.RentalDays);
            var payment = await _paymentService.InitiatePaymentAsync(billingId, amount, method);

            string redirectUrl = "";

            switch (method)
            {
                case PaymentMethod.Zarinpal:
                    redirectUrl = $"https://www.zarinpal.com/pg/StartPay/{payment.TransactionId}";
                    break;
                case PaymentMethod.BankPasargad:
                    redirectUrl = $"https://bankpasargad.com/payment/{payment.TransactionId}";
                    break;
            }

            return Redirect(redirectUrl);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string authority, string status)
        {
            var payment = await _paymentService.GetPaymentByTransactionIdAsync(authority);

            if (payment == null)
            {
                return View("PaymentError", new PaymentErrorViewModel
                {
                    Message = "تراکنش یافت نشد"
                });
            }

            if (status.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                var verifiedPayment = await _paymentService.VerifyPaymentAsync(authority);

                if (verifiedPayment.Status == PaymentStatus.Completed)
                {
                    // ایجاد سفارش اجاره سرور
                    var order = await _serverRentalService.CreateOrderAsync(
                        verifiedPayment.Id,
                        1, // اینجا باید ID سرور واقعی قرار گیرد
                        verifiedPayment.BillingInformation.RentalDays);

                    return View("PaymentSuccess", new PaymentSuccessViewModel
                    {
                        OrderId = order.Id,
                        Amount = verifiedPayment.Amount,
                        PaymentDate = verifiedPayment.PaymentDate
                    });
                }
            }

            return View("PaymentError", new PaymentErrorViewModel
            {
                Message = "پرداخت با خطا مواجه شد"
            });
        }

        private decimal CalculateRentalAmount(int rentalDays)
        {
            // محاسبه مبلغ بر اساس تعداد روزهای اجاره
            // این منطق می‌تواند بر اساس نیازهای کسب‌وکار تغییر کند
            decimal basePrice = 1000000; // قیمت پایه برای 30 روز
            return (basePrice / 30) * rentalDays;
        }

    }
}
