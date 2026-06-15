using MultiShop.DtoLayer.OrderDtos.OrderAddressDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.OrderServices.OrderAddressServices
{
    public class OrderAddressService : IOrderAddressService
    {
        private readonly HttpClient _httpClient;
        public OrderAddressService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateOrderAddressAsync(CreateOrderAddressDto createOrderAddressDto)
        {
            await _httpClient.PostAsJsonAsync<CreateOrderAddressDto>("addresses", createOrderAddressDto);
        }

        public async Task<List<ResultOrderAddressByEmailDto>> GetDistinctAddressesByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new List<ResultOrderAddressByEmailDto>();
            }

            var response = await _httpClient.GetAsync($"addresses/ByEmailDistinct?email={Uri.EscapeDataString(email)}");
            if (!response.IsSuccessStatusCode)
            {
                return new List<ResultOrderAddressByEmailDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<ResultOrderAddressByEmailDto>();
            }

            return JsonConvert.DeserializeObject<List<ResultOrderAddressByEmailDto>>(json) ?? new List<ResultOrderAddressByEmailDto>();
        }
    }
}
