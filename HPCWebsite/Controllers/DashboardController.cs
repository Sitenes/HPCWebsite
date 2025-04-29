using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;
using ViewModel;

namespace HPCWebsite.Controllers
{
    [Authorize]

    public class DashboardController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IPaymentService _paymentService;
        private readonly IServerRentalService _serverRentalService;
        private readonly IUserService _userManager;
        private readonly IShoppingCartService _cartService;
        private readonly IServerService _serverService;

        public DashboardController(
            IBillingService billingService,
            IPaymentService paymentService,
            IServerRentalService serverRentalService,
            IUserService userManager,
            IShoppingCartService cartService,
            IServerService serverService
            )
        {
            _billingService = billingService;
            _paymentService = paymentService;
            _serverRentalService = serverRentalService;
            _userManager = userManager;
            _cartService = cartService;
            _serverService = serverService;
        }
        public IActionResult Index()
        {
            return RedirectToAction("ServerList");
        }
        public async Task<IActionResult> ServerList()
        {

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // دریافت سرورهای کاربر از سرویس
            var userServers = await _serverRentalService.GetUserServersAsync(userId);
            return View(userServers);

        }
        public IActionResult ServerInfo()
        {
            return View();
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var billingInfo = await _billingService.GetUserBillingInformationAsync(userId);
            var cart = await _cartService.GetUserCartAsync(userId);

            var model = new CheckoutViewModel
            {
                BillingInformation = billingInfo ?? new BillingInformation(),
                ShoppingCart = cart
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var billingInfo = await _billingService.SaveBillingInformationAsync(model.BillingInformation, userId);

            // در اینجا می‌توانید کاربر را به صفحه پرداخت هدایت کنید
            return RedirectToAction("Checkout");
        }

        [HttpGet]
        public async Task<IActionResult> Payment(int billingId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
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
        public async Task<IActionResult> PaymentCallback(string authority, string status = "OK")
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
