using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.DiscountServices;

namespace MultiShop.WebUI.Controllers
{
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IBasketService _basketService;
        public DiscountController(IDiscountService discountService, IBasketService basketService)
        {
            _discountService = discountService;
            _basketService = basketService;
        }

        [HttpGet]
        public PartialViewResult ConfirmDiscountCoupon()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDiscountCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["BasketError"] = "Lütfen bir kupon kodu giriniz.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var values = await _discountService.GetDiscountCouponCountRate(code);
            if (values <= 0)
            {
                TempData["BasketError"] = "Kupon kodu geçersiz veya süresi dolmuş.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var basketValues = await _basketService.GetBasket();
            if (basketValues == null || basketValues.BasketItems == null || basketValues.BasketItems.Count == 0)
            {
                TempData["BasketError"] = "Sepet boş olduğu için kupon uygulanamadı.";
                return RedirectToAction("Index", "ShoppingCart");
            }
            var totalPriceWithTax = basketValues.TotalPrice + basketValues.TotalPrice / 100 * 10;

            var totalNewPriceWithDiscount = totalPriceWithTax - (totalPriceWithTax / 100 * values);

            return RedirectToAction("Index", "ShoppingCart", new { code = code, discountRate = values, totalNewPriceWithDiscount = totalNewPriceWithDiscount });
        }
    }
}
