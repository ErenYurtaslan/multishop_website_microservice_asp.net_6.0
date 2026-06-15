using MultiShop.DtoLayer.CatalogDtos.ProductDetailDtos;
using Newtonsoft.Json;
using System.Net;

namespace MultiShop.WebUI.Services.CatalogServices.ProductDetailServices
{
    public class ProductDetailService : IProductDetailService
    {
        private readonly HttpClient _httpClient;
        public ProductDetailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateProductDetailAsync(CreateProductDetailDto createProductDetailDto)
        {
            await _httpClient.PostAsJsonAsync<CreateProductDetailDto>("productdetails", createProductDetailDto);
        }
        public async Task DeleteProductDetailAsync(string id)
        {
            await _httpClient.DeleteAsync("productdetails?id=" + id);
        }
        public async Task<GetByIdProductDetailDto> GetByIdProductDetailAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productdetails/" + id);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new GetByIdProductDetailDto
                {
                    ProductDetailId = string.Empty,
                    ProductDescription = string.Empty,
                    ProductInfo = string.Empty,
                    ProductId = id
                };
            }

            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return new GetByIdProductDetailDto
                {
                    ProductDetailId = string.Empty,
                    ProductDescription = string.Empty,
                    ProductInfo = string.Empty,
                    ProductId = id
                };
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new GetByIdProductDetailDto
                {
                    ProductDetailId = string.Empty,
                    ProductDescription = string.Empty,
                    ProductInfo = string.Empty,
                    ProductId = id
                };
            }

            var values = JsonConvert.DeserializeObject<GetByIdProductDetailDto>(jsonData);
            return values ?? new GetByIdProductDetailDto
            {
                ProductDetailId = string.Empty,
                ProductDescription = string.Empty,
                ProductInfo = string.Empty,
                ProductId = id
            };
        }
        public async Task<List<ResultProductDetailDto>> GetAllProductDetailAsync()
        {
            var responseMessage = await _httpClient.GetAsync("productdetails");
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<ResultProductDetailDto>>(jsonData);
            return values;
        }
        public async Task UpdateProductDetailAsync(UpdateProductDetailDto updateProductDetailDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateProductDetailDto>("productdetails", updateProductDetailDto);
        }

        public async Task<GetByIdProductDetailDto> GetByProductIdProductDetailAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productdetails/GetProductDetailByProductId/" + id);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new GetByIdProductDetailDto
                {
                    ProductDetailId = string.Empty,
                    ProductDescription = string.Empty,
                    ProductInfo = string.Empty,
                    ProductId = id
                };
            }

            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return new GetByIdProductDetailDto
                {
                    ProductDetailId = string.Empty,
                    ProductDescription = string.Empty,
                    ProductInfo = string.Empty,
                    ProductId = id
                };
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new GetByIdProductDetailDto
                {
                    ProductDetailId = string.Empty,
                    ProductDescription = string.Empty,
                    ProductInfo = string.Empty,
                    ProductId = id
                };
            }

            var values = JsonConvert.DeserializeObject<GetByIdProductDetailDto>(jsonData);
            return values ?? new GetByIdProductDetailDto
            {
                ProductDetailId = string.Empty,
                ProductDescription = string.Empty,
                ProductInfo = string.Empty,
                ProductId = id
            };
        }
    }
}
