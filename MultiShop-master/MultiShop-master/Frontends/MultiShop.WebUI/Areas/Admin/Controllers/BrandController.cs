using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.BrandDtos;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Brand")]
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BrandController(IBrandService brandService, IWebHostEnvironment webHostEnvironment)
        {
            _brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }

        void BrandViewBagList()
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Markalar";
            ViewBag.v3 = "Marka Listesi";
            ViewBag.v0 = "Marka İşlemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            BrandViewBagList();
            var values = await _brandService.GetAllBrandAsync();
            return View(values);
        }

        [HttpGet]
        [Route("CreateBrand")]
        public IActionResult CreateBrand()
        {
            BrandViewBagList();
            return View();
        }

        [HttpPost]
        [Route("CreateBrand")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBrand(CreateBrandDto createBrandDto, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(createBrandDto.BrandName))
            {
                ModelState.AddModelError(nameof(createBrandDto.BrandName), "Marka adı zorunludur.");
                BrandViewBagList();
                return View(createBrandDto);
            }

            createBrandDto.BrandName = createBrandDto.BrandName.Trim();
            createBrandDto.ImageUrl = await ResolveBrandImageUrlAsync(imageFile, createBrandDto.ImageUrl);
            await _brandService.CreateBrandAsync(createBrandDto);
            return RedirectToAction("Index", "Brand", new { area = "Admin" });
        }

        [Route("DeleteBrand/{id}")]
        public async Task<IActionResult> DeleteBrand(string id)
        {
            await _brandService.DeleteBrandAsync(id);
            return RedirectToAction("Index", "Brand", new { area = "Admin" });
        }

        [Route("UpdateBrand/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateBrand(string id)
        {
            BrandViewBagList();
            var values = await _brandService.GetByIdBrandAsync(id);
            return View(values);
        }

        [Route("UpdateBrand/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBrand(UpdateBrandDto updateBrandDto, IFormFile? imageFile)
        {
            if (string.IsNullOrWhiteSpace(updateBrandDto.BrandName))
            {
                ModelState.AddModelError(nameof(updateBrandDto.BrandName), "Marka adı zorunludur.");
                BrandViewBagList();
                return View(updateBrandDto);
            }

            updateBrandDto.BrandName = updateBrandDto.BrandName.Trim();
            updateBrandDto.ImageUrl = await ResolveBrandImageUrlAsync(imageFile, updateBrandDto.ImageUrl);
            await _brandService.UpdateBrandAsync(updateBrandDto);
            return RedirectToAction("Index", "Brand", new { area = "Admin" });
        }

        private async Task<string> ResolveBrandImageUrlAsync(IFormFile? imageFile, string? existingOrManualUrl)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return (existingOrManualUrl ?? string.Empty).Trim();
            }

            var extension = Path.GetExtension(imageFile.FileName);
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
            if (string.IsNullOrWhiteSpace(extension) ||
                !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return (existingOrManualUrl ?? string.Empty).Trim();
            }

            var brandsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "brands");
            Directory.CreateDirectory(brandsFolder);

            var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(brandsFolder, safeFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(fileStream);

            return $"/images/brands/{safeFileName}";
        }
    }
}
