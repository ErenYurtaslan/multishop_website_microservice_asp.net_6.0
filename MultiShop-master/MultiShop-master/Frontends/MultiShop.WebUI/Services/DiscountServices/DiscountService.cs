using MultiShop.DtoLayer.DiscountDtos;

namespace MultiShop.WebUI.Services.DiscountServices
{
    public class DiscountService : IDiscountService
    {
        private readonly HttpClient _httpClient;
        public DiscountService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<GetDiscountCodeDetailByCode> GetDiscountCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return null;
            }

            var responseMessage = await _httpClient.GetAsync($"discounts/GetCodeDetailByCodeAsync?code={Uri.EscapeDataString(code)}");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return null;
            }
            var values = await responseMessage.Content.ReadFromJsonAsync<GetDiscountCodeDetailByCode>();
            return values;
        }

        public async Task<int> GetDiscountCouponCountRate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return 0;
            }

            var responseMessage = await _httpClient.GetAsync($"discounts/GetDiscountCouponCountRate?code={Uri.EscapeDataString(code)}");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return 0;
            }

            var values = await responseMessage.Content.ReadFromJsonAsync<int>();
            return values;
        }
    }
}
