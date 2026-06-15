using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.WebUI.Areas.User.Models;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOderingServices;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class MyOrderController : Controller
    {
        private readonly IOrderOderingService _orderOderingService;
        private readonly IUserService _userService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;
        public MyOrderController(
            IOrderOderingService orderOderingService,
            IUserService userService,
            IOrderDetailService orderDetailService,
            IProductService productService)
        {
            _orderOderingService = orderOderingService;
            _userService = userService;
            _orderDetailService = orderDetailService;
            _productService = productService;
        }

        public Task<IActionResult> Index() => MyOrderList();

        public async Task<IActionResult> MyOrderList()
        {
            try
            {
                ViewBag.directory1 = "Ana Sayfa";
                ViewBag.directory2 = "Sayfalar";
                ViewBag.directory3 = "Siparişlerim";

                var user = await _userService.GetUserInfo();
                if (user == null || string.IsNullOrWhiteSpace(user.Id))
                {
                    return RedirectToAction("Index", "Login", new { area = "" });
                }
                var values = await _orderOderingService.GetOrderingByUserId(user.Id);
                var allDetails = await _orderDetailService.GetAllOrderDetailsAsync();
                var myOrderIds = (values ?? new()).Select(x => x.OrderingId).ToHashSet();
                var myDetails = (allDetails ?? new()).Where(x => myOrderIds.Contains(x.OrderingId)).ToList();

                var productIds = myDetails
                    .Where(x => !string.IsNullOrWhiteSpace(x.ProductId))
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();

                var productLookup = new Dictionary<string, MultiShop.DtoLayer.CatalogDtos.ProductDtos.UpdateProductDto>();
                foreach (var productId in productIds)
                {
                    var product = await _productService.GetByIdProductAsync(productId);
                    if (product != null)
                    {
                        productLookup[productId] = product;
                    }
                }

                var previewsByOrder = myDetails
                    .GroupBy(x => x.OrderingId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(d =>
                        {
                            productLookup.TryGetValue(d.ProductId ?? string.Empty, out var p);
                            return new UserOrderProductPreviewViewModel
                            {
                                ProductId = d.ProductId ?? string.Empty,
                                ProductName = !string.IsNullOrWhiteSpace(d.ProductName)
                                    ? d.ProductName
                                    : (p?.ProductName ?? "Ürün"),
                                ProductImageUrl = !string.IsNullOrWhiteSpace(p?.ProductImageUrl)
                                    ? p.ProductImageUrl
                                    : "/images/placeholder-product.svg",
                                Quantity = d.ProductAmount
                            };
                        }).ToList());

                ViewBag.OrderProductPreviews = previewsByOrder;
                return View(values);
            }
            catch
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }
        }
    }
}
