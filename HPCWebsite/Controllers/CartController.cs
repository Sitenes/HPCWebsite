using Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;
using ViewModel;

namespace HPCWebsite.Controllers
{
    public class CartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUserService _userService;

        public CartController(
            IShoppingCartService shoppingCartService,
            IUserService userService)
        {
            _shoppingCartService = shoppingCartService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int serverId, int rentalDays)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Login", "User"), message = "لطفاً ابتدا وارد سیستم شوید" });
                }

                await _shoppingCartService.AddToCartAsync(userId, serverId, rentalDays, int.Parse(User.FindFirstValue(ClaimTypes.Name) ?? "1"));
                var cartItemCount = await _shoppingCartService.GetCartItemCountAsync(userId);

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Checkout","Dashboard"),
                    message = "سرور با موفقیت به سبد خرید اضافه شد",
                    cartItemCount = cartItemCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "خطا در افزودن به سبد خرید"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _shoppingCartService.GetUserCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _shoppingCartService.RemoveFromCartAsync(userId, itemId);

                return Json(new { success = true, message = "آیتم با موفقیت حذف شد" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "خطا در حذف آیتم" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateItem(int itemId, int rentalDays)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                if (userId == 0)
                {
                    return Json(new { success = false, message = "ابتدا وارد حساب کاربری شوید" });
                }

                if (rentalDays < 1)
                {
                    return Json(new { success = false, message = "تعداد روز اجاره معتبر نیست" });
                }

                await _shoppingCartService.UpdateCartItemAsync(userId, itemId, rentalDays);
                

                return Json(new { success = true, message = "آیتم مورد نظر یافت نشد یا به‌روزرسانی نشد" });
            }
            catch
            {
                return Json(new { success = false, message = "خطا در به‌روزرسانی آیتم" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var cart = await _shoppingCartService.GetUserCartAsync(userId);
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                return Json(new { success = true, items = new List<object>(), total = 0 });
            }

            return Json(new
            {
                success = true,
                items = cart.Items.Select(x => new
                {
                    x.Id,
                    x.ServerName,
                    x.ImageUrl,
                    rentalDays = x.RentalDays,
                    dailyPrice = x.DailyPrice,
                    totalPrice = x.TotalPrice
                }),
                total = cart.Total
            });
        }

    }
}
