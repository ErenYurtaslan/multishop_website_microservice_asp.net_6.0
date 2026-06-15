using MultiShop.DtoLayer.CatalogDtos.ProductImageDtos;
using Newtonsoft.Json;
using System.Net;

namespace MultiShop.WebUI.Services.CatalogServices.ProductImageServices
{
    public class ProductImageService:IProductImageService
    {
        private readonly HttpClient _httpClient;
        public ProductImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateProductImageAsync(CreateProductImageDto createProductImageDto)
        {
            await _httpClient.PostAsJsonAsync<CreateProductImageDto>("productimages", createProductImageDto);
        }
        public async Task DeleteProductImageAsync(string id)
        {
            await _httpClient.DeleteAsync("productimages?id=" + id);
        }
        public async Task<GetByIdProductImageDto> GetByIdProductImageAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productimages/" + id);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new GetByIdProductImageDto
                {
                    ProductImageID = string.Empty,
                    Image1 = string.Empty,
                    Image2 = string.Empty,
                    Image3 = string.Empty,
                    Image4 = string.Empty,
                    ProductId = id
                };
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new GetByIdProductImageDto
                {
                    ProductImageID = string.Empty,
                    Image1 = string.Empty,
                    Image2 = string.Empty,
                    Image3 = string.Empty,
                    Image4 = string.Empty,
                    ProductId = id
                };
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new GetByIdProductImageDto
                {
                    ProductImageID = string.Empty,
                    Image1 = string.Empty,
                    Image2 = string.Empty,
                    Image3 = string.Empty,
                    Image4 = string.Empty,
                    ProductId = id
                };
            }

            var values = JsonConvert.DeserializeObject<GetByIdProductImageDto>(jsonData);
            return values ?? new GetByIdProductImageDto
            {
                ProductImageID = string.Empty,
                Image1 = string.Empty,
                Image2 = string.Empty,
                Image3 = string.Empty,
                Image4 = string.Empty,
                ProductId = id
            };
        }
        public async Task<List<ResultProductImageDto>> GetAllProductImageAsync()
        {
            var responseMessage = await _httpClient.GetAsync("productimages");
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return new List<ResultProductImageDto>();
            }
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultProductImageDto>();
            }
            var values = JsonConvert.DeserializeObject<List<ResultProductImageDto>>(jsonData);
            return values ?? new List<ResultProductImageDto>();
        }
        public async Task UpdateProductImageAsync(UpdateProductImageDto updateProductImageDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateProductImageDto>("productimages", updateProductImageDto);
        }

        public async Task<GetByIdProductImageDto> GetByProductIdProductImageAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("productimages/ProductImagesByProductId/" + id);
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new GetByIdProductImageDto
                {
                    ProductImageID = string.Empty,
                    Image1 = string.Empty,
                    Image2 = string.Empty,
                    Image3 = string.Empty,
                    Image4 = string.Empty,
                    ProductId = id
                };
            }

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new GetByIdProductImageDto
                {
                    ProductImageID = string.Empty,
                    Image1 = string.Empty,
                    Image2 = string.Empty,
                    Image3 = string.Empty,
                    Image4 = string.Empty,
                    ProductId = id
                };
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new GetByIdProductImageDto
                {
                    ProductImageID = string.Empty,
                    Image1 = string.Empty,
                    Image2 = string.Empty,
                    Image3 = string.Empty,
                    Image4 = string.Empty,
                    ProductId = id
                };
            }

            var values = JsonConvert.DeserializeObject<GetByIdProductImageDto>(jsonData);
            return values ?? new GetByIdProductImageDto
            {
                ProductImageID = string.Empty,
                Image1 = string.Empty,
                Image2 = string.Empty,
                Image3 = string.Empty,
                Image4 = string.Empty,
                ProductId = id
            };
        }

       
    }
}
