using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductImageDtos;
using MultiShop.WebUI.Services.CatalogServices.ProductImageServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/ProductImage")]
    public class ProductImageController : Controller
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        [Route("ProductImageDetail/{id}")]
        [HttpGet]
        public async Task<IActionResult> ProductImageDetail(string id)
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Ürünler";
            ViewBag.v3 = "Ürün Görsel Güncelleme Sayfası";
            ViewBag.v0 = "Ürün Görsel İşlemleri";

            var current = await _productImageService.GetByProductIdProductImageAsync(id);
            var model = new UpdateProductImageDto
            {
                ProductImageID = current?.ProductImageID ?? string.Empty,
                ProductId = string.IsNullOrWhiteSpace(current?.ProductId) ? id : current.ProductId,
                Image1 = current?.Image1 ?? string.Empty,
                Image2 = current?.Image2 ?? string.Empty,
                Image3 = current?.Image3 ?? string.Empty,
                Image4 = current?.Image4 ?? string.Empty
            };
            return View(model);
        }

        [Route("ProductImageDetail/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductImageDetail(UpdateProductImageDto updateProductImageDto)
        {
            if (string.IsNullOrWhiteSpace(updateProductImageDto.ProductImageID))
            {
                await _productImageService.CreateProductImageAsync(new CreateProductImageDto
                {
                    ProductId = updateProductImageDto.ProductId,
                    Image1 = updateProductImageDto.Image1,
                    Image2 = updateProductImageDto.Image2,
                    Image3 = updateProductImageDto.Image3,
                    Image4 = updateProductImageDto.Image4
                });
            }
            else
            {
                await _productImageService.UpdateProductImageAsync(updateProductImageDto);
            }

            return RedirectToAction("ProductListWithCategory", "Product", new { area = "Admin" });
        }
    }
}
