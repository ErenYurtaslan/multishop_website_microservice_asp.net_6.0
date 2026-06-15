using MultiShop.DtoLayer.OrderDtos.OrderDetailDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.OrderServices.OrderDetailServices
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly HttpClient _httpClient;

        public OrderDetailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ResultOrderDetailDto>> GetAllOrderDetailsAsync()
        {
            var responseMessage = await _httpClient.GetAsync("orderdetails");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultOrderDetailDto>();
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultOrderDetailDto>();
            }

            var values = JsonConvert.DeserializeObject<List<ResultOrderDetailDto>>(jsonData);
            return values ?? new List<ResultOrderDetailDto>();
        }
    }
}
