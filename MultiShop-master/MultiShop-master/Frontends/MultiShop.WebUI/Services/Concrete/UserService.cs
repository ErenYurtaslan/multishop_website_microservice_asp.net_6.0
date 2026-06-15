using MultiShop.WebUI.Models;
using MultiShop.WebUI.Services.Interfaces;

namespace MultiShop.WebUI.Services.Concrete
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserDetailViewModel> GetUserInfo()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDetailViewModel>("/api/users/getuser");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<List<UserDetailViewModel>> GetAllUserList()
        {
            try
            {
                var values = await _httpClient.GetFromJsonAsync<List<UserDetailViewModel>>("/api/users/getalluserlist");
                return values ?? new List<UserDetailViewModel>();
            }
            catch (HttpRequestException)
            {
                return new List<UserDetailViewModel>();
            }
        }
    }
}
