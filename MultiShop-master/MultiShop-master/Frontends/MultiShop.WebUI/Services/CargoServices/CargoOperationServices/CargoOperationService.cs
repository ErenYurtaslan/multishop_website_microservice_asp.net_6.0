using MultiShop.DtoLayer.CargoDtos.CargoOperationDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CargoServices.CargoOperationServices
{
    public class CargoOperationService : ICargoOperationService
    {
        private readonly HttpClient _httpClient;

        public CargoOperationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task CreateCargoOperationAsync(CreateCargoOperationDto createCargoOperationDto)
        {
            var response = await _httpClient.PostAsJsonAsync("CargoOperations", createCargoOperationDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<ResultCargoOperationDto>> GetAllCargoOperationsAsync()
        {
            var responseMessage = await _httpClient.GetAsync("CargoOperations");
            if (!responseMessage.IsSuccessStatusCode)
            {
                return new List<ResultCargoOperationDto>();
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return new List<ResultCargoOperationDto>();
            }

            var values = JsonConvert.DeserializeObject<List<ResultCargoOperationDto>>(jsonData);
            return values ?? new List<ResultCargoOperationDto>();
        }
    }
}
