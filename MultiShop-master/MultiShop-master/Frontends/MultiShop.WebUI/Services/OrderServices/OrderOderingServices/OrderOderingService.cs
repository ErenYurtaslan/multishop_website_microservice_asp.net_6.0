using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using MultiShop.DtoLayer.OrderDtos.OrderOrderingDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.OrderServices.OrderOderingServices
{
    public class OrderOderingService : IOrderOderingService
    {
        private readonly HttpClient _httpClient;
        public OrderOderingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<ResultOrderingByUserIdDto>> GetAllOrderingsAsync()
        {
            var responseMessage = await _httpClient.GetAsync("orderings");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultOrderingByUserIdDto>();
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultOrderingByUserIdDto>();
            }

            var values = JsonConvert.DeserializeObject<List<ResultOrderingByUserIdDto>>(jsonData);
            return values ?? new List<ResultOrderingByUserIdDto>();
        }

        public async Task<List<ResultOrderingByUserIdDto>> GetOrderingByUserId(string id)
        {
            //$"products/ProductListWithCategoryByCategoryId/{CategoryId}"
            var responseMessage = await _httpClient.GetAsync($"orderings/GetOrderingByUserId/{id}");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultOrderingByUserIdDto>();
            }
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultOrderingByUserIdDto>();
            }
            var values = JsonConvert.DeserializeObject<List<ResultOrderingByUserIdDto>>(jsonData);
            return values ?? new List<ResultOrderingByUserIdDto>();
        }

        public async Task<int?> CreateOrderingAndGetIdAsync(CreateOrderingDto createOrderingDto)
        {
            var createResponse = await _httpClient.PostAsJsonAsync("orderings", createOrderingDto);
            if (!createResponse.IsSuccessStatusCode)
            {
                var body = await createResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Order create failed. Status={(int)createResponse.StatusCode}; Body={body}");
            }

            var list = await GetOrderingByUserId(createOrderingDto.UserId);
            var latest = list
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.OrderingId)
                .FirstOrDefault();

            return latest?.OrderingId;
        }

        public async Task CreateOrderDetailAsync(CreateOrderDetailDto createOrderDetailDto)
        {
            var response = await _httpClient.PostAsJsonAsync("orderdetails", createOrderDetailDto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Order detail create failed. Status={(int)response.StatusCode}; Body={body}");
            }
        }
    }
}
