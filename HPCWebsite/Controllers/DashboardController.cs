using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
                BillingInformation = billingInfo ?? new HpcBillingInformation(),
                ShoppingCart = cart
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            //if (!ModelState.IsValid)
            //
            //    return View(model);
            //}
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.Name) ?? "1");
            if (model.BillingInformation.UserId == 0)
            {
                model.BillingInformation.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            }
            var billingInfo = await _billingService.SaveBillingInformationAsync(model.BillingInformation, userId);

            // در اینجا می‌توانید کاربر را به صفحه پرداخت هدایت کنید
            return RedirectToAction("Checkout");
        }

        //[HttpGet]
        //public async Task<IActionResult> Payment(int billingId)
        //{
        //    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        //    var billingInfo = await _billingService.GetUserBillingInformationAsync(userId);

        //    if (billingInfo == null || billingInfo.Id != billingId)
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    // محاسبه مبلغ بر اساس مدت اجاره و نوع سرور
        //    decimal amount = 

        //    ViewBag.BillingId = billingId;
        //    ViewBag.Amount = amount;

        //    return View();
        //}
        [HttpGet]
        public async Task<IActionResult> InitiatePayment(int userId, PaymentMethod method = PaymentMethod.Zarinpal)
        {
            if (userId == 0)
                userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var userCart = await _cartService.GetUserCartAsync(userId);
            var user = await _userManager.GetByIdAsync(userId);

            if (userCart == null)
            {
                return RedirectToAction("Index");
            }
            decimal amount = userCart.Total;
          

            if (method == PaymentMethod.Zarinpal)
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                var merchantId = config["Zarinpal:MerchantId"];
                var baseUrl = config["Zarinpal:BaseUrl"];

                var callbackUrl = Url.Action("PaymentCallback", null, null, Request.Scheme);

                using var client = new HttpClient();
                var req = new ZarinpalRequest
                {
                    merchant_id = merchantId,
                    amount = (int)amount * 10,
                    callback_url = callbackUrl,
                    description = "خرید سرور",
                    metadata = new Dictionary<string, string>
            {
                { "mobile", ("0" + user?.Mobile.ToString()) ?? "" },
                { "email", user?.Email ?? "" }
            }
                };

                var response = await client.PostAsJsonAsync($"{baseUrl}payment/request.json", req);
                var result = await response.Content.ReadFromJsonAsync<ZarinpalResponse>();

                if (result != null && result.data != null && response.IsSuccessStatusCode)
                {
                    var authority = result.data.authority;
                    var payment = await _paymentService.InitiatePaymentAsync(userCart.Id, authority, amount, method, int.Parse(User.FindFirstValue(ClaimTypes.Name) ?? "1"));
                    return Redirect($"https://sandbox.zarinpal.com/pg/StartPay/{authority}");
                }
                else
                {
                    return View("PaymentError", new PaymentErrorViewModel
                    {
                        Message = $"خطا در اتصال به زرین‌پال: {result?.message ?? "نامشخص"}"
                    });
                }
            }

            return RedirectToAction("Checkout");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string authority, string status = "OK")
        {
            var payment = await _paymentService.GetPaymentByTransactionIdAsync(authority);

            if (payment == null)
            {
                return View("PaymentFail", new PaymentErrorViewModel
                {
                    Message = "تراکنش یافت نشد"
                });
            }

            if (status.Equals("OK", StringComparison.OrdinalIgnoreCase))
            {
                var verifiedPayment = await _paymentService.VerifyPaymentAsync(authority);

                if (verifiedPayment.Status == PaymentStatus.Completed)
                {
                    var cart = verifiedPayment.ShoppingCart;
                    var orders = new List<HpcServerRentalOrder>();

                    if (cart?.Items != null && cart.Items.Any())
                    {
                        foreach (var item in cart.Items)
                        {
                            var order = await _serverRentalService.CreateOrderAsync(
                                verifiedPayment.ShoppingCart.UserId,
                                item.ServerId,
                                item.RentalDays,
                                int.Parse(User.FindFirstValue(ClaimTypes.Name) ?? "1")
                            );

                            orders.Add(order);
                        }
                    }

                    return View("PaymentSuccess", new PaymentSuccessViewModel
                    {
                        OrderId = verifiedPayment.ShoppingCart.Id,
                        Amount = verifiedPayment.Amount,
                        PaymentDate = verifiedPayment.PaymentDate
                    });
                }
            }

            return View("PaymentFail", new PaymentErrorViewModel
            {
                Message = "پرداخت با خطا مواجه شد"
            });
        }

    }
}
