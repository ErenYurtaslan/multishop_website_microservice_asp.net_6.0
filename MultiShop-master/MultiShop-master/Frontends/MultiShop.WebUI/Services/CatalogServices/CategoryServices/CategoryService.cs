using MultiShop.DtoLayer.CatalogDtos.CategoryDtos;
using Newtonsoft.Json;

namespace MultiShop.WebUI.Services.CatalogServices.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        public CategoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            await _httpClient.PostAsJsonAsync<CreateCategoryDto>("categories", createCategoryDto);
        }
        public async Task DeleteCategoryAsync(string id)
        {
            await _httpClient.DeleteAsync("categories?id=" + id);
        }
        public async Task<UpdateCategoryDto> GetByIdCategoryAsync(string id)
        {
            var responseMessage = await _httpClient.GetAsync("categories/" + id);
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return null;
            var json = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonConvert.DeserializeObject<UpdateCategoryDto>(json);
        }
        public async Task<List<ResultCategoryDto>> GetAllCategoryAsync()
        {
            var responseMessage = await _httpClient.GetAsync("categories");
            if (!responseMessage.IsSuccessStatusCode || responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
                return new List<ResultCategoryDto>();
            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData)) return new List<ResultCategoryDto>();
            return JsonConvert.DeserializeObject<List<ResultCategoryDto>>(jsonData) ?? new List<ResultCategoryDto>();
        }
        public async Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
        {
            await _httpClient.PutAsJsonAsync<UpdateCategoryDto>("categories", updateCategoryDto);
        }
    }
}
