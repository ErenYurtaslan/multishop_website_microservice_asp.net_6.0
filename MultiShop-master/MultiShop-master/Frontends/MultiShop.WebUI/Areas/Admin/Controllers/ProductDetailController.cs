using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos;
using MultiShop.WebUI.Services.CatalogServices.ProductDetailServices;

namespace MultiShop.WebUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    [Route("Admin/ProductDetail")]
    public class ProductDetailController : Controller
    {
        private readonly IProductDetailService _productDetailService;

        public ProductDetailController(IProductDetailService productDetailService)
        {
            _productDetailService = productDetailService;
        }

        [Route("UpdateProductDetail/{id}")]
        [HttpGet]
        public async Task<IActionResult> UpdateProductDetail(string id)
        {
            ViewBag.v1 = "Ana Sayfa";
            ViewBag.v2 = "Ürünler";
            ViewBag.v3 = "Ürün Açıklama ve Bilgi Güncelleme Sayfası";
            ViewBag.v0 = "Ürün İşlemleri";

            var current = await _productDetailService.GetByProductIdProductDetailAsync(id);
            var model = new UpdateProductDetailDto
            {
                ProductDetailId = current?.ProductDetailId ?? string.Empty,
                ProductId = string.IsNullOrWhiteSpace(current?.ProductId) ? id : current.ProductId,
                ProductDescription = current?.ProductDescription ?? string.Empty,
                ProductInfo = current?.ProductInfo ?? string.Empty
            };
            return View(model);
        }

        [Route("UpdateProductDetail/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProductDetail(UpdateProductDetailDto updateProductDetailDto)
        {
            if (string.IsNullOrWhiteSpace(updateProductDetailDto.ProductDetailId))
            {
                await _productDetailService.CreateProductDetailAsync(new CreateProductDetailDto
                {
                    ProductId = updateProductDetailDto.ProductId,
                    ProductDescription = updateProductDetailDto.ProductDescription,
                    ProductInfo = updateProductDetailDto.ProductInfo
                });
            }
            else
            {
                await _productDetailService.UpdateProductDetailAsync(updateProductDetailDto);
            }

            return RedirectToAction("ProductListWithCategory", "Product", new { area = "Admin" });
        }
    }
}
