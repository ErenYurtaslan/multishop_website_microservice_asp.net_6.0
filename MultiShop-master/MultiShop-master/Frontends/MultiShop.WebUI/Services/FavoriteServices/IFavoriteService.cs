using MultiShop.DtoLayer.FavoriteDtos;

namespace MultiShop.WebUI.Services.FavoriteServices
{
    public interface IFavoriteService
    {
        Task<List<ResultUserFavoriteDto>> GetMyFavoritesAsync();
        Task<int> GetMyFavoritesCountAsync();
        Task<bool> IsFavoriteAsync(string productId);
        Task<bool> AddFavoriteAsync(CreateUserFavoriteDto dto);
        Task<bool> RemoveFavoriteAsync(int id);
        Task<bool> RemoveFavoriteByProductAsync(string productId);
    }
}
