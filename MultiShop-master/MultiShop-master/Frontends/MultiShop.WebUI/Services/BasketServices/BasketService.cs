using System.Text;
using System.Net;
using MultiShop.DtoLayer.BasketDtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MultiShop.WebUI.Services.BasketServices
{
    /// <summary>
    /// Gateway basket API + Newtonsoft (same style as FavoriteService) for robust camelCase JSON.
    /// </summary>
    public class BasketService : IBasketService
    {
        private static readonly JsonSerializerSettings SerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly HttpClient _httpClient;
        public BasketService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AddBasketItem(BasketItemDto basketItemDto)
        {
            var values = await GetBasket() ?? new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
            values.BasketItems ??= new List<BasketItemDto>();

            var existing = values.BasketItems.FirstOrDefault(x => x.ProductId == basketItemDto.ProductId);
            if (existing == null)
            {
                if (basketItemDto.Quantity <= 0) basketItemDto.Quantity = 1;
                values.BasketItems.Add(basketItemDto);
            }
            else
            {
                existing.Quantity += basketItemDto.Quantity > 0 ? basketItemDto.Quantity : 1;
            }

            await SaveBasket(values);
        }

        public async Task DeleteBasket(string userId)
        {
            _ = userId;
            var response = await _httpClient.DeleteAsync("baskets");
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new HttpRequestException("Basket delete unauthorized.", null, response.StatusCode);
            }

            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Basket delete failed. Status={(int)response.StatusCode}; Body={body}",
                null,
                response.StatusCode);
        }

        public async Task<BasketTotalDto> GetBasket()
        {
            var responseMessage = await _httpClient.GetAsync("baskets");
            if (!responseMessage.IsSuccessStatusCode)
            {
                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized ||
                    responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new HttpRequestException("Basket read unauthorized.", null, responseMessage.StatusCode);
                }
                return new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
            }

            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
            }

            try
            {
                var values = JsonConvert.DeserializeObject<BasketTotalDto>(json, SerializerSettings);
                if (values == null)
                {
                    return new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
                }
                values.BasketItems ??= new List<BasketItemDto>();
                return values;
            }
            catch (JsonException)
            {
                return new BasketTotalDto { BasketItems = new List<BasketItemDto>() };
            }
        }

        public async Task<bool> RemoveBasketItem(string productId)
        {
            var values = await GetBasket();
            if (values == null || values.BasketItems == null)
            {
                return false;
            }
            var deletedItem = values.BasketItems.FirstOrDefault(x => x.ProductId == productId);
            if (deletedItem == null)
            {
                return false;
            }
            values.BasketItems.Remove(deletedItem);
            await SaveBasket(values);
            return true;
        }

        public async Task SaveBasket(BasketTotalDto basketTotalDto)
        {
            var json = JsonConvert.SerializeObject(basketTotalDto, SerializerSettings);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("baskets", content);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var message = $"Basket save failed. Status={(int)response.StatusCode}; Body={responseBody}";
                throw new HttpRequestException(message, null, response.StatusCode);
            }
        }
    }
}
