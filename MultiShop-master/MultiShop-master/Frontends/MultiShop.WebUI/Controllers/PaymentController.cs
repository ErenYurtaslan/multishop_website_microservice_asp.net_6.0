using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CargoDtos.CargoDetailDtos;
using MultiShop.DtoLayer.CargoDtos.CargoOperationDtos;
using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CargoServices.CargoDetailServices;
using MultiShop.WebUI.Services.CargoServices.CargoOperationServices;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderOderingServices;
using MultiShop.WebUI.Services.PaymentServices;
using System.Globalization;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IBasketService _basketService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderOderingService _orderOderingService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        private readonly ICargoDetailService _cargoDetailService;
        private readonly ICargoOperationService _cargoOperationService;

        public PaymentController(
            IBasketService basketService,
            IPaymentService paymentService,
            IOrderOderingService orderOderingService,
            IUserService userService,
            IProductService productService,
            IBrandService brandService,
            ICargoDetailService cargoDetailService,
            ICargoOperationService cargoOperationService)
        {
            _basketService = basketService;
            _paymentService = paymentService;
            _orderOderingService = orderOderingService;
            _userService = userService;
            _productService = productService;
            _brandService = brandService;
            _cargoDetailService = cargoDetailService;
            _cargoOperationService = cargoOperationService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "Ödeme Ekranı";
            ViewBag.directory3 = "Kartla Ödeme";

            var basket = await _basketService.GetBasket();
            if (basket == null || basket.BasketItems == null || basket.BasketItems.Count == 0)
            {
                TempData["BasketError"] = "Sepetin boş, ödeme yapamazsın.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var stockValidationError = await ValidateBasketStockAsync(basket.BasketItems);
            if (!string.IsNullOrWhiteSpace(stockValidationError))
            {
                TempData["BasketError"] = stockValidationError;
                return RedirectToAction("Index", "ShoppingCart");
            }

            var totalPrice = basket.TotalPrice;
            var tax = totalPrice / 100m * 10m;
            ViewBag.basketItems = basket.BasketItems;
            ViewBag.itemCount = basket.BasketItems.Sum(x => x.Quantity);
            ViewBag.subTotal = totalPrice;
            ViewBag.tax = tax;
            ViewBag.grandTotal = totalPrice + tax;
            await PopulatePaymentBrandLookupAsync(basket.BasketItems);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(string cardNumber, string cardHolder, string expMonth, string expYear, string cvv)
        {
            var basket = await _basketService.GetBasket();
            if (basket == null || basket.BasketItems == null || basket.BasketItems.Count == 0)
            {
                TempData["BasketError"] = "Sepetin boş, ödeme yapamazsın.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            // Sunum amacli minimal validasyon — gercek bir odeme entegrasyonu yapmiyoruz.
            cardNumber = (cardNumber ?? string.Empty).Replace(" ", string.Empty).Trim();
            cardHolder = (cardHolder ?? string.Empty).Trim();
            cvv = (cvv ?? string.Empty).Trim();

            if (cardNumber.Length < 12 || cardNumber.Length > 19 || !cardNumber.All(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "Kart numarası geçersiz.");
            }
            if (string.IsNullOrWhiteSpace(cardHolder))
            {
                ModelState.AddModelError(string.Empty, "Kart sahibi boş bırakılamaz.");
            }
            if (cvv.Length is < 3 or > 4 || !cvv.All(char.IsDigit))
            {
                ModelState.AddModelError(string.Empty, "CVV geçersiz.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.directory1 = "MultiShop";
                ViewBag.directory2 = "Ödeme Ekranı";
                ViewBag.directory3 = "Kartla Ödeme";

                var totalPriceErr = basket.TotalPrice;
                var taxErr = totalPriceErr / 100m * 10m;
                ViewBag.basketItems = basket.BasketItems;
                ViewBag.itemCount = basket.BasketItems.Sum(x => x.Quantity);
                ViewBag.subTotal = totalPriceErr;
                ViewBag.tax = taxErr;
                ViewBag.grandTotal = totalPriceErr + taxErr;
                await PopulatePaymentBrandLookupAsync(basket.BasketItems);
                return View("Index");
            }

            var subTotal = basket.TotalPrice;
            var tax = subTotal / 100m * 10m;
            var grand = subTotal + tax;

            var summary = string.Join(", ", basket.BasketItems.Select(i => $"{i.ProductName} x{i.Quantity}"));
            var last4 = cardNumber.Length >= 4 ? cardNumber[^4..] : cardNumber;

            var result = await _paymentService.CreatePaymentAsync(new CreatePaymentDto
            {
                TotalAmount = grand,
                CardLast4 = last4,
                CardHolderName = cardHolder,
                OrderSummary = summary
            });

            if (result == null)
            {
                TempData["BasketError"] = "Ödeme alınırken bir hata oluştu, lütfen tekrar dene.";
                return RedirectToAction("Index");
            }

            try
            {
                var currentUser = await _userService.GetUserInfo();
                var userId = currentUser?.Id;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    userId = basket.UserId;
                }
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new InvalidOperationException("Kullanıcı kimliği bulunamadı.");
                }

                var orderingId = await _orderOderingService.CreateOrderingAndGetIdAsync(new CreateOrderingDto
                {
                    UserId = userId,
                    TotalPrice = grand,
                    OrderDate = DateTime.UtcNow
                });

                if (orderingId.HasValue)
                {
                    foreach (var item in basket.BasketItems)
                    {
                        await _orderOderingService.CreateOrderDetailAsync(new CreateOrderDetailDto
                        {
                            OrderingId = orderingId.Value,
                            ProductId = item.ProductId ?? string.Empty,
                            ProductName = item.ProductName ?? string.Empty,
                            ProductPrice = item.Price,
                            ProductAmount = item.Quantity,
                            ProductTotalPrice = item.Price * item.Quantity
                        });
                    }

                    await DecreaseProductStocksAsync(basket.BasketItems);
                    await CreateCargoRecordsAsync(currentUser?.Name, currentUser?.Surname, userId);
                }
            }
            catch
            {
                TempData["PaymentOrderWarning"] = "Ödeme alındı fakat sipariş kaydı oluşturulurken teknik bir hata oluştu.";
            }

            // Sepeti bosalt
            try
            {
                await _basketService.SaveBasket(new MultiShop.DtoLayer.BasketDtos.BasketTotalDto
                {
                    UserId = basket.UserId,
                    DiscountCode = string.Empty,
                    DiscountRate = 0,
                    BasketItems = new List<MultiShop.DtoLayer.BasketDtos.BasketItemDto>()
                });
            }
            catch
            {
                // Sepet temizlemesi basarisiz olsa bile odeme alindi sayilir.
            }

            TempData["PaymentResultId"] = result.PaymentRecordId;
            TempData["PaymentAmount"] = result.TotalAmount.ToString("0.##");
            TempData["PaymentLast4"] = result.CardLast4;
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "Sipariş";
            ViewBag.directory3 = "Onay";
            return View();
        }

        private async Task PopulatePaymentBrandLookupAsync(List<MultiShop.DtoLayer.BasketDtos.BasketItemDto> basketItems)
        {
            var products = await _productService.GetAllProductAsync();
            var brands = await _brandService.GetAllBrandAsync();

            var productBrandMap = (products ?? new())
                .Where(x => !string.IsNullOrWhiteSpace(x?.ProductId))
                .ToDictionary(x => x.ProductId!, x => x.BrandId);
            var brandLookup = (brands ?? new()).ToDictionary(x => x.BrandId, x => x);

            ViewBag.BrandByProductId = (basketItems ?? new())
                .Where(x => !string.IsNullOrWhiteSpace(x.ProductId))
                .ToDictionary(
                    x => x.ProductId,
                    x =>
                    {
                        if (productBrandMap.TryGetValue(x.ProductId, out var brandId) &&
                            !string.IsNullOrWhiteSpace(brandId) &&
                            brandLookup.TryGetValue(brandId, out var brand))
                        {
                            return brand;
                        }
                        return null;
                    });
        }

        private async Task<string?> ValidateBasketStockAsync(List<MultiShop.DtoLayer.BasketDtos.BasketItemDto> basketItems)
        {
            foreach (var item in basketItems)
            {
                if (string.IsNullOrWhiteSpace(item.ProductId))
                {
                    return "Sepette ürün bilgisi eksik, lütfen sepetini güncelle.";
                }

                var product = await _productService.GetByIdProductAsync(item.ProductId);
                if (product == null)
                {
                    return $"\"{item.ProductName}\" ürünü artık bulunamıyor.";
                }

                if (product.Stock < item.Quantity)
                {
                    return $"\"{item.ProductName}\" için stok yetersiz (kalan: {product.Stock}).";
                }
            }

            return null;
        }

        private async Task DecreaseProductStocksAsync(List<MultiShop.DtoLayer.BasketDtos.BasketItemDto> basketItems)
        {
            foreach (var item in basketItems)
            {
                if (string.IsNullOrWhiteSpace(item.ProductId)) continue;

                var product = await _productService.GetByIdProductAsync(item.ProductId);
                if (product == null) continue;

                product.Stock = Math.Max(0, product.Stock - item.Quantity);
                await _productService.UpdateProductAsync(product);
            }
        }

        private async Task CreateCargoRecordsAsync(string? currentUserName, string? currentUserSurname, string? userId)
        {
            var selectedCargoCompanyId = ResolveSelectedCargoCompanyId();
            if (!selectedCargoCompanyId.HasValue)
            {
                return;
            }

            var receiver = $"{currentUserName} {currentUserSurname}".Trim();
            if (string.IsNullOrWhiteSpace(receiver))
            {
                receiver = string.IsNullOrWhiteSpace(userId) ? "Musteri" : userId;
            }

            var barcode = GenerateCargoBarcode();
            await _cargoDetailService.CreateCargoDetailAsync(new CreateCargoDetailDto
            {
                SenderCustomer = "MultiShop Depo",
                ReceiverCustomer = receiver,
                CargoCompanyId = selectedCargoCompanyId.Value,
                Barcode = barcode
            });

            await _cargoOperationService.CreateCargoOperationAsync(new CreateCargoOperationDto
            {
                Barcode = barcode.ToString(CultureInfo.InvariantCulture),
                Description = "Siparis alindi, kargo sureci baslatildi.",
                OperationDate = DateTime.UtcNow
            });
        }

        private int? ResolveSelectedCargoCompanyId()
        {
            if (TempData.Peek("SelectedCargoCompanyId") is int idFromInt && idFromInt > 0)
            {
                return idFromInt;
            }

            if (TempData.Peek("SelectedCargoCompanyId") is string idFromString &&
                int.TryParse(idFromString, out var parsedId) &&
                parsedId > 0)
            {
                return parsedId;
            }

            return null;
        }

        private static int GenerateCargoBarcode()
        {
            var nowPart = int.Parse(DateTime.UtcNow.ToString("HHmmss", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
            var randomPart = Random.Shared.Next(100, 999);
            return nowPart * 1000 + randomPart;
        }
    }
}
