using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.WebUI.Areas.User.Models;
using MultiShop.WebUI.Services.CargoServices.CargoCompanyServices;
using MultiShop.WebUI.Services.CargoServices.CargoDetailServices;
using MultiShop.WebUI.Services.CargoServices.CargoOperationServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderDetailServices;
using MultiShop.WebUI.Services.OrderServices.OrderOderingServices;
using System.Globalization;

namespace MultiShop.WebUI.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class CargoController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICargoDetailService _cargoDetailService;
        private readonly ICargoOperationService _cargoOperationService;
        private readonly ICargoCompanyService _cargoCompanyService;
        private readonly IOrderOderingService _orderOderingService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;

        public CargoController(
            IUserService userService,
            ICargoDetailService cargoDetailService,
            ICargoOperationService cargoOperationService,
            ICargoCompanyService cargoCompanyService,
            IOrderOderingService orderOderingService,
            IOrderDetailService orderDetailService,
            IProductService productService)
        {
            _userService = userService;
            _cargoDetailService = cargoDetailService;
            _cargoOperationService = cargoOperationService;
            _cargoCompanyService = cargoCompanyService;
            _orderOderingService = orderOderingService;
            _orderDetailService = orderDetailService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.directory1 = "Ana Sayfa";
            ViewBag.directory2 = "Sayfalar";
            ViewBag.directory3 = "Kargom Nerede";

            var vm = new UserCargoTrackingViewModel();
            var user = await _userService.GetUserInfo();
            if (user == null)
            {
                return RedirectToAction("Index", "Login", new { area = "" });
            }

            var fullName = $"{user.Name} {user.Surname}".Trim();
            var details = await _cargoDetailService.GetAllCargoDetailsAsync();
            var operations = await _cargoOperationService.GetAllCargoOperationsAsync();
            var companies = await _cargoCompanyService.GetAllCargoCompanyAsync();
            var companyLookup = (companies ?? new()).ToDictionary(x => x.cargoCompanyId, x => x.cargoCompanyName);

            var myDetails = (details ?? new())
                .Where(x => IsCargoBelongsToUser(x.ReceiverCustomer, fullName, user.Id))
                .OrderByDescending(x => x.CargoDetailId)
                .ToList();

            foreach (var detail in myDetails)
            {
                var barcodeString = detail.Barcode.ToString(CultureInfo.InvariantCulture);
                var steps = (operations ?? new())
                    .Where(x => string.Equals((x.Barcode ?? string.Empty).Trim(), barcodeString, StringComparison.Ordinal))
                    .OrderByDescending(x => x.OperationDate)
                    .Select(x => new CargoOperationStepViewModel
                    {
                        Description = x.Description,
                        OperationDate = x.OperationDate
                    })
                    .ToList();

                var latest = steps.FirstOrDefault();
                vm.CargoCards.Add(new CargoTrackingCardViewModel
                {
                    CargoDetailId = detail.CargoDetailId,
                    Barcode = detail.Barcode,
                    SenderCustomer = detail.SenderCustomer,
                    ReceiverCustomer = detail.ReceiverCustomer,
                    CargoCompanyName = companyLookup.TryGetValue(detail.CargoCompanyId, out var companyName) ? companyName : "Bilinmeyen Kargo",
                    LastStatusDescription = latest?.Description ?? "Kargo kaydı oluşturuldu.",
                    LastOperationDate = latest?.OperationDate,
                    Steps = steps
                });
            }

            var myOrders = await _orderOderingService.GetOrderingByUserId(user.Id);
            var sortedOrders = (myOrders ?? new())
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.OrderingId)
                .ToList();
            var myOrderIds = sortedOrders.Select(x => x.OrderingId).ToHashSet();
            var allDetails = await _orderDetailService.GetAllOrderDetailsAsync();
            var myOrderDetails = (allDetails ?? new())
                .Where(x => myOrderIds.Contains(x.OrderingId))
                .OrderByDescending(x => x.OrderDetailId)
                .ToList();

            var productCache = new Dictionary<string, MultiShop.DtoLayer.CatalogDtos.ProductDtos.UpdateProductDto>();
            async Task<UserOrderProductPreviewViewModel> BuildPreviewAsync(ResultOrderDetailDto d)
            {
                MultiShop.DtoLayer.CatalogDtos.ProductDtos.UpdateProductDto? product = null;
                if (!string.IsNullOrWhiteSpace(d.ProductId))
                {
                    if (!productCache.TryGetValue(d.ProductId, out var cached))
                    {
                        cached = await _productService.GetByIdProductAsync(d.ProductId);
                        if (cached != null)
                        {
                            productCache[d.ProductId] = cached;
                        }
                    }
                    product = cached;
                }

                return new UserOrderProductPreviewViewModel
                {
                    ProductId = d.ProductId ?? string.Empty,
                    ProductName = !string.IsNullOrWhiteSpace(d.ProductName) ? d.ProductName : (product?.ProductName ?? "Ürün"),
                    ProductImageUrl = !string.IsNullOrWhiteSpace(product?.ProductImageUrl) ? product.ProductImageUrl : "/images/placeholder-product.svg",
                    Quantity = d.ProductAmount
                };
            }

            var productsByOrder = new Dictionary<int, List<UserOrderProductPreviewViewModel>>();
            foreach (var order in sortedOrders)
            {
                var orderDetails = myOrderDetails
                    .Where(x => x.OrderingId == order.OrderingId)
                    .ToList();
                var previews = new List<UserOrderProductPreviewViewModel>();
                foreach (var d in orderDetails)
                {
                    previews.Add(await BuildPreviewAsync(d));
                }
                productsByOrder[order.OrderingId] = previews;
            }

            // Basit ve deterministic eslesme: en yeni kargo karti <-> en yeni siparis.
            for (var i = 0; i < vm.CargoCards.Count; i++)
            {
                if (i < sortedOrders.Count &&
                    productsByOrder.TryGetValue(sortedOrders[i].OrderingId, out var previews))
                {
                    vm.CargoCards[i].Products = previews;
                }
            }

            return View(vm);
        }

        private static bool IsCargoBelongsToUser(string? receiverCustomer, string fullName, string? userId)
        {
            if (string.IsNullOrWhiteSpace(receiverCustomer))
            {
                return false;
            }

            var receiver = receiverCustomer.Trim();
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                if (receiver.Equals(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (receiver.Contains(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(userId) && receiver.Contains(userId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
