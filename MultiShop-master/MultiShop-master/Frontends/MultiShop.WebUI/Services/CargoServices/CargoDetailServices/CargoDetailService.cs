using MultiShop.DtoLayer.CargoDtos.CargoDetailDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CargoServices.CargoDetailServices
{
    public class CargoDetailService : ICargoDetailService
    {
        private readonly HttpClient _httpClient;

        public CargoDetailService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateCargoDetailAsync(CreateCargoDetailDto createCargoDetailDto)
        {
            var response = await _httpClient.PostAsJsonAsync("CargoDetails", createCargoDetailDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ResultCargoDetailDto>> GetAllCargoDetailsAsync()
        {
            var responseMessage = await _httpClient.GetAsync("CargoDetails");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultCargoDetailDto>();
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultCargoDetailDto>();
            }

            var values = JsonConvert.DeserializeObject<List<ResultCargoDetailDto>>(jsonData);
            return values ?? new List<ResultCargoDetailDto>();
        }
    }
}
