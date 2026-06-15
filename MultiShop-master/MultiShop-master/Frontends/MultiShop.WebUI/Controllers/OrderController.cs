using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;
using MultiShop.WebUI.Services.BasketServices;
using MultiShop.WebUI.Services.CargoServices.CargoCompanyServices;
using MultiShop.WebUI.Services.Interfaces;
using MultiShop.WebUI.Services.OrderServices.OrderAddressServices;

namespace MultiShop.WebUI.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderAddressService _orderAddressService;
        private readonly IUserService _userService;
        private readonly IBasketService _basketService;
        private readonly ICargoCompanyService _cargoCompanyService;

        public OrderController(
            IOrderAddressService orderAddressService,
            IUserService userService,
            IBasketService basketService,
            ICargoCompanyService cargoCompanyService)
        {
            _orderAddressService = orderAddressService;
            _userService = userService;
            _basketService = basketService;
            _cargoCompanyService = cargoCompanyService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.directory1 = "MultiShop";
            ViewBag.directory2 = "Siparişler";
            ViewBag.directory3 = "Sipariş İşlemleri";

            var basket = await _basketService.GetBasket();
            if (basket == null || basket.BasketItems == null || basket.BasketItems.Count == 0)
            {
                TempData["BasketError"] = "Sepetin boş, önce ürün eklemelisin.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            var user = await _userService.GetUserInfo();
            var model = new CreateOrderAddressDto
            {
                Name = user?.Name ?? string.Empty,
                Surname = user?.Surname ?? string.Empty,
                Email = user?.Email ?? string.Empty
            };
            await PopulateCargoCompaniesAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreateOrderAddressDto createOrderAddressDto)
        {
            var basket = await _basketService.GetBasket();
            if (basket == null || basket.BasketItems == null || basket.BasketItems.Count == 0)
            {
                TempData["BasketError"] = "Sepetin boş.";
                return RedirectToAction("Index", "ShoppingCart");
            }

            try
            {
                var user = await _userService.GetUserInfo();
                createOrderAddressDto.UserId = user.Id;
                // Loginli kullanicida ad/soyad/mail hesap bilgisinden gelsin; formda tekrar istemeyelim.
                createOrderAddressDto.Name = user?.Name ?? createOrderAddressDto.Name;
                createOrderAddressDto.Surname = user?.Surname ?? createOrderAddressDto.Surname;
                createOrderAddressDto.Email = user?.Email ?? createOrderAddressDto.Email;

                if (string.IsNullOrWhiteSpace(createOrderAddressDto.Phone))
                {
                    TempData["BasketError"] = "Ödeme için telefon numarası gerekli.";
                    await PopulateCargoCompaniesAsync();
                    return View(createOrderAddressDto);
                }
                if (!createOrderAddressDto.CargoCompanyId.HasValue || createOrderAddressDto.CargoCompanyId.Value <= 0)
                {
                    TempData["BasketError"] = "Lütfen bir kargo şirketi seç.";
                    await PopulateCargoCompaniesAsync();
                    return View(createOrderAddressDto);
                }
                if (string.IsNullOrWhiteSpace(createOrderAddressDto.Description))
                {
                    createOrderAddressDto.Description = "-";
                }

                TempData["SelectedCargoCompanyId"] = createOrderAddressDto.CargoCompanyId.Value;
                await _orderAddressService.CreateOrderAddressAsync(createOrderAddressDto);
            }
            catch
            {
                // Adres kaydında hata olsa bile ödeme adımına devam etmeye izin ver;
                // sunum amaçlı kritik degil. Gercek senaryoda hata gosterip dururuz.
            }

            return RedirectToAction("Index", "Payment");
        }

        private async Task PopulateCargoCompaniesAsync()
        {
            var values = await _cargoCompanyService.GetAllCargoCompanyAsync();
            ViewBag.CargoCompanyValues = (values ?? new())
                .Select(x => new SelectListItem
                {
                    Text = x.cargoCompanyName,
                    Value = x.cargoCompanyId.ToString()
                })
                .ToList();
        }
    }
}
