using System.Net;
using System.Text;
using MultiShop.DtoLayer.CatalogDtos.ProductDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CatalogServices.ProductServices
{
    public class ProductService : IProductService
    {
        private const string ProductPlaceholder = "/images/placeholder-product.svg";
        private readonly HttpClient _httpClient;
        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateProductAsync(CreateProductDto createProductDto)
        {
            ApplyProductNormalization(createProductDto);
            var response = await _httpClient.PostAsJsonAsync<CreateProductDto>("products", createProductDto);
            await EnsureSuccessAsync(response, "Product create failed");
        }
        public async Task DeleteProductAsync(string id)
        {
            await _httpClient.DeleteAsync("products?id=" + id);
        }
        public async Task<UpdateProductDto> GetByIdProductAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("products/" + id);
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return null;
            }

            var product = JsonConvert.DeserializeObject<UpdateProductDto>(jsonData);
            ApplyProductNormalization(product);
            return product;
        }
        public async Task<List<ResultProductDto>> GetAllProductAsync()
        {
            var responseMessage = await _httpClient.GetAsync("products");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultProductDto>();
            }
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultProductDto>();
            }
            var values = JsonConvert.DeserializeObject<List<ResultProductDto>>(jsonData) ?? new List<ResultProductDto>();
            ApplyProductNormalization(values);
            return values;
        }
        public async Task UpdateProductAsync(UpdateProductDto updateProductDto)
        {
            ApplyProductNormalization(updateProductDto);
            var response = await _httpClient.PutAsJsonAsync<UpdateProductDto>("products", updateProductDto);
            await EnsureSuccessAsync(response, "Product update failed");
        }

        public async Task<List<ResultProductWithCategoryDto>> GetProductsWithCategoryAsync()
        {
            var responseMessage = await _httpClient.GetAsync("products/ProductListWithCategory");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultProductWithCategoryDto>();
            }
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultProductWithCategoryDto>();
            }
            var values = JsonConvert.DeserializeObject<List<ResultProductWithCategoryDto>>(jsonData) ?? new List<ResultProductWithCategoryDto>();
            ApplyProductNormalization(values);
            return values;
        }

        public async Task<List<ResultProductWithCategoryDto>> GetProductsWithCategoryByCatetegoryIdAsync(string CategoryId)
        {
            var responseMessage = await _httpClient.GetAsync($"products/ProductListWithCategoryByCategoryId/{CategoryId}");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultProductWithCategoryDto>();
            }
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultProductWithCategoryDto>();
            }
            var values = JsonConvert.DeserializeObject<List<ResultProductWithCategoryDto>>(jsonData) ?? new List<ResultProductWithCategoryDto>();
            ApplyProductNormalization(values);
            return values;
        }

        public async Task<List<ResultProductWithCategoryDto>> GetFilteredProductsAsync(ProductFilterRequestDto filter)
        {
            filter ??= new ProductFilterRequestDto();
            var query = BuildFilterQueryString(filter);

            var responseMessage = await _httpClient.GetAsync($"products/Filter{query}");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultProductWithCategoryDto>();
            }
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultProductWithCategoryDto>();
            }
            var values = JsonConvert.DeserializeObject<List<ResultProductWithCategoryDto>>(jsonData) ?? new List<ResultProductWithCategoryDto>();
            ApplyProductNormalization(values);
            return values;
        }

        public async Task<List<string>> GetDistinctColorsAsync()
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync("products/Colors");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return new List<string>();
                }
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return new List<string>();
                }
                return JsonConvert.DeserializeObject<List<string>>(jsonData) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> GetDistinctSizesAsync()
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync("products/Sizes");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    return new List<string>();
                }
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    return new List<string>();
                }
                return JsonConvert.DeserializeObject<List<string>>(jsonData) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string BuildFilterQueryString(ProductFilterRequestDto filter)
        {
            var sb = new StringBuilder();
            void Append(string key, string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                sb.Append(sb.Length == 0 ? '?' : '&');
                sb.Append(key);
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(value));
            }

            Append("CategoryId", filter.CategoryId ?? string.Empty);
            Append("Color", filter.Color ?? string.Empty);
            Append("Size", filter.Size ?? string.Empty);
            Append("Search", filter.Search ?? string.Empty);
            if (filter.MinPrice.HasValue)
                Append("MinPrice", filter.MinPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (filter.MaxPrice.HasValue)
                Append("MaxPrice", filter.MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (filter.InStockOnly)
                Append("InStockOnly", "true");

            return sb.ToString();
        }

        private static void ApplyProductNormalization(CreateProductDto? product)
        {
            if (product is null) return;
            product.ProductImageUrl = NormalizeProductImageUrl(product.ProductImageUrl);
        }

        private static void ApplyProductNormalization(UpdateProductDto? product)
        {
            if (product is null) return;
            product.ProductImageUrl = NormalizeProductImageUrl(product.ProductImageUrl);
        }

        private static void ApplyProductNormalization(IEnumerable<ResultProductDto>? products)
        {
            if (products is null) return;
            foreach (var product in products)
            {
                if (product is null) continue;
                product.ProductImageUrl = NormalizeProductImageUrl(product.ProductImageUrl);
            }
        }

        private static void ApplyProductNormalization(IEnumerable<ResultProductWithCategoryDto>? products)
        {
            if (products is null) return;
            foreach (var product in products)
            {
                if (product is null) continue;
                product.ProductImageUrl = NormalizeProductImageUrl(product.ProductImageUrl);
            }
        }

        private static string NormalizeProductImageUrl(string? rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl))
            {
                return ProductPlaceholder;
            }

            var value = rawUrl.Trim().Replace('\\', '/');

            if (value.StartsWith("~/", StringComparison.Ordinal))
            {
                value = value[1..];
            }

            if (value.StartsWith("//", StringComparison.Ordinal))
            {
                return $"https:{value}";
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var absolute) &&
                (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                return absolute.ToString();
            }

            if (value.StartsWith("/", StringComparison.Ordinal))
            {
                return value;
            }

            return "/" + value.TrimStart('/');
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, string prefix)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{prefix}. Status={(int)response.StatusCode}; Body={body}", null, response.StatusCode);
        }
    }
}