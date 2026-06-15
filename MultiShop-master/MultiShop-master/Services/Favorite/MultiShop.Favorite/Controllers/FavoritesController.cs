using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Favorite.Context;
using MultiShop.Favorite.Entities;
using System.Security.Claims;

namespace MultiShop.Favorite.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoriteContext _context;

        public FavoritesController(FavoriteContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue("sub")
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("MyFavorites")]
        public async Task<IActionResult> MyFavorites()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var values = await _context.UserFavorites
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return Ok(values);
        }

        [HttpGet("MyFavoritesCount")]
        public async Task<IActionResult> MyFavoritesCount()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Ok(0);
            }

            var count = await _context.UserFavorites.CountAsync(x => x.UserId == userId);
            return Ok(count);
        }

        [HttpGet("IsFavorite/{productId}")]
        public async Task<IActionResult> IsFavorite(string productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Ok(false);
            }

            var exists = await _context.UserFavorites
                .AnyAsync(x => x.UserId == userId && x.ProductId == productId);
            return Ok(exists);
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite(UserFavorite userFavorite)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            userFavorite.UserId = userId;
            userFavorite.CreatedDate = DateTime.UtcNow;
            userFavorite.UserFavoriteId = 0;

            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == userFavorite.ProductId);

            if (existing != null)
            {
                return Ok(new { message = "Ürün zaten favorilerinizde.", id = existing.UserFavoriteId });
            }

            _context.UserFavorites.Add(userFavorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ürün favorilere eklendi.", id = userFavorite.UserFavoriteId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFavorite(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var fav = await _context.UserFavorites
                .FirstOrDefaultAsync(x => x.UserFavoriteId == id && x.UserId == userId);

            if (fav == null)
            {
                return NotFound();
            }

            _context.UserFavorites.Remove(fav);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Favori başarıyla kaldırıldı." });
        }

        [HttpDelete("ByProduct/{productId}")]
        public async Task<IActionResult> RemoveFavoriteByProduct(string productId)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var fav = await _context.UserFavorites
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

            if (fav == null)
            {
                return NotFound();
            }

            _context.UserFavorites.Remove(fav);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Favori başarıyla kaldırıldı." });
        }
    }
}
