using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using MultiShop.WebUI.Services.CatalogServices.BrandServices;
using MultiShop.WebUI.Services.CatalogServices.CategoryServices;
using MultiShop.WebUI.Services.CatalogServices.ProductServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/Product")]
    public class ProductController : Controller
    {
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _webHostEnvironment = webHostEnvironment;
        }
        void ProductViewBagList()
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Ürünler";
            ViewBag.v3 = "Ürün Listesi";
            ViewBag.v0 = "Ürün İşlemleri";
        }

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            ProductViewBagList();
            var values = await _productService.GetAllProductAsync();
            var brands = await _brandService.GetAllBrandAsync();
            ViewBag.BrandLookup = (brands ?? new()).ToDictionary(x => x.BrandId, x => x);
            return View(values);
        }

        [Route("ProductListWithCategory")]
        public async Task<IActionResult> ProductListWithCategory()
        {
            ProductViewBagList();

            //var client = _httpClientFactory.CreateClient();
            //var responseMessage = await client.GetAsync("https://localhost:7070/api/Products/ProductListWithCategory");
            //if (responseMessage.IsSuccessStatusCode)
            //{
            //    var jsonData = await responseMessage.Content.ReadAsStringAsync();
            //    var values = JsonConvert.DeserializeObject<List<ResultProductWithCategoryDto>>(jsonData);
            //    return View(values);
            //}
            return View();
        }

        [Route("CreateProduct")]
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ProductViewBagList();
            await PopulateCategoryValuesAsync();
            await PopulateBrandValuesAsync();
            return View();
        }

        [HttpPost]
        [Route("CreateProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(CreateProductDto createProductDto, IFormFile? productImageFile)
        {
            if (createProductDto.Stock < 0) createProductDto.Stock = 0;
            if (string.IsNullOrWhiteSpace(createProductDto.BrandId)) createProductDto.BrandId = null;
            if (string.IsNullOrWhiteSpace(createProductDto.ColorCode)) createProductDto.ColorCode = null;
            createProductDto.ProductDescription = createProductDto.ProductDescription?.Trim() ?? string.Empty;
            createProductDto.Color = createProductDto.Color?.Trim() ?? string.Empty;
            createProductDto.Size = createProductDto.Size?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(createProductDto.CategoryId))
            {
                ProductViewBagList();
                await PopulateCategoryValuesAsync();
                await PopulateBrandValuesAsync();
                ModelState.AddModelError(string.Empty, "Ürün kategorisi seçmelisin.");
                return View(createProductDto);
            }
            createProductDto.ProductImageUrl = await SaveProductImageAsync(productImageFile, createProductDto.ProductImageUrl);
            try
            {
                await _productService.CreateProductAsync(createProductDto);
                TempData["AdminProductInfo"] = "Ürün başarıyla eklendi.";
                return RedirectToAction("Index", "Product", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                ProductViewBagList();
                await PopulateCategoryValuesAsync();
                await PopulateBrandValuesAsync();
                ModelState.AddModelError(string.Empty, $"Ürün eklenemedi: {ex.Message}");
                return View(createProductDto);
            }
        }

        [Route("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index", "Product", new { area = "Admin" });
        }

        [Route("UpdateProduct/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string id)
        {
            ProductViewBagList();
            await PopulateCategoryValuesAsync();
            await PopulateBrandValuesAsync();

            var productValues = await _productService.GetByIdProductAsync(id);
            if (productValues == null)
            {
                productValues = new UpdateProductDto { ProductId = id };
            }
            return View(productValues);
        }

        [Route("UpdateProduct/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(UpdateProductDto updateProductDto, IFormFile? productImageFile)
        {
            if (updateProductDto.Stock < 0) updateProductDto.Stock = 0;
            if (string.IsNullOrWhiteSpace(updateProductDto.BrandId)) updateProductDto.BrandId = null;
            if (string.IsNullOrWhiteSpace(updateProductDto.ColorCode)) updateProductDto.ColorCode = null;
            updateProductDto.ProductDescription = updateProductDto.ProductDescription?.Trim() ?? string.Empty;
            updateProductDto.Color = updateProductDto.Color?.Trim() ?? string.Empty;
            updateProductDto.Size = updateProductDto.Size?.Trim() ?? string.Empty;
            updateProductDto.ProductImageUrl = await SaveProductImageAsync(productImageFile, updateProductDto.ProductImageUrl);
            try
            {
                await _productService.UpdateProductAsync(updateProductDto);
                TempData["AdminProductInfo"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction("Index", "Product", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                ProductViewBagList();
                await PopulateCategoryValuesAsync();
                await PopulateBrandValuesAsync();
                ModelState.AddModelError(string.Empty, $"Ürün güncellenemedi: {ex.Message}");
                return View(updateProductDto);
            }
        }

        private async Task<string> SaveProductImageAsync(IFormFile? imageFile, string? currentValue)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return currentValue ?? string.Empty;
            }

            var extension = Path.GetExtension(imageFile.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
            {
                return currentValue ?? string.Empty;
            }

            var uploadsRoot = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }

        private async Task PopulateCategoryValuesAsync()
        {
            var values = await _categoryService.GetAllCategoryAsync();
            List<SelectListItem> categoryValues = (from x in values
                                                   select new SelectListItem
                                                   {
                                                       Text = x.CategoryName,
                                                       Value = x.CategoryID
                                                   }).ToList();
            ViewBag.CategoryValues = categoryValues;
        }

        private async Task PopulateBrandValuesAsync()
        {
            var values = await _brandService.GetAllBrandAsync();
            var brandValues = (values ?? new())
                .Select(x => new SelectListItem
                {
                    Text = x.BrandName,
                    Value = x.BrandId
                })
                .ToList();

            ViewBag.BrandValues = brandValues;
        }
    }
}
