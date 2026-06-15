using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.BasketDtos;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductService _productService;
        private readonly IBasketService _basketService;

        public ShoppingCartController(IProductService productService, IBasketService basketService)
        {
            _productService = productService;
            _basketService = basketService;
        }

        [Authorize]
        public async Task<IActionResult> Index(string code, int discountRate, decimal totalNewPriceWithDiscount)
        {
            ViewBag.code = code;
            ViewBag.discountRate = discountRate;
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Ürünler";
            ViewBag.directory3 = "Sepetim";

            var values = await _basketService.GetBasket() ?? new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
            values.BasketItems ??= new List<BasketItemDto>();

            var totalPrice = values.TotalPrice;
            var tax = totalPrice / 100m * 10m;
            var totalPriceWithTax = totalPrice + tax;
            var finalTotal = totalNewPriceWithDiscount > 0 ? totalNewPriceWithDiscount : totalPriceWithTax;

            ViewBag.total = totalPrice;
            ViewBag.tax = tax;
            ViewBag.totalPriceWithTax = totalPriceWithTax;
            ViewBag.totalNewPriceWithDiscount = finalTotal;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> AddBasketItem(string id, int quantity = 1, string returnUrl = null)
        {
            var product = await _productService.GetByIdProductAsync(id);
            if (product == null)
            {
                TempData["BasketError"] = "Ürün bulunamadı.";
                return RedirectSafe(returnUrl);
            }

            if (product.Stock <= 0)
            {
                TempData["BasketError"] = $"\"{product.ProductName}\" stokta yok, sepete eklenemiyor.";
                return RedirectSafe(returnUrl);
            }

            var basket = await _basketService.GetBasket() ?? new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
            var existing = basket.BasketItems?.FirstOrDefault(x => x.ProductId == id);
            var currentQty = existing?.Quantity ?? 0;
            if (quantity < 1) quantity = 1;
            if (quantity > product.Stock) quantity = product.Stock;

            if (currentQty + quantity > product.Stock)
            {
                TempData["BasketError"] = $"\"{product.ProductName}\" için stok yetersiz (mevcut: {product.Stock} adet).";
                return RedirectSafe(returnUrl);
            }

            var item = new BasketItemDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.ProductPrice,
                Quantity = quantity,
                ProductImageUrl = product.ProductImageUrl
            };
            try
            {
                await _basketService.AddBasketItem(item);
                TempData["BasketAdded"] = product.ProductName;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var target = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? returnUrl
                    : Url.Action("Index", "Default");
                return RedirectToAction("Index", "Login", new { returnUrl = target });
            }
            catch (HttpRequestException ex)
            {
                var status = ex.StatusCode.HasValue ? ((int)ex.StatusCode.Value).ToString() : "n/a";
                var msg = ex.Message ?? string.Empty;
                if (msg.Length > 180) msg = msg.Substring(0, 180) + "...";
                var details = string.IsNullOrWhiteSpace(msg) ? "" : $" (detay: {msg})";
                TempData["BasketError"] = $"Sepet kaydedilemedi (HTTP {status}). Ağ/oturum sorunu olabilir{details}";
            }

            return RedirectSafe(returnUrl);
        }

        [Authorize]
        public async Task<IActionResult> RemoveBasketItem(string id)
        {
            await _basketService.RemoveBasketItem(id);
            return RedirectToAction("Index");
        }

        private IActionResult RedirectSafe(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index");
        }
    }
}
