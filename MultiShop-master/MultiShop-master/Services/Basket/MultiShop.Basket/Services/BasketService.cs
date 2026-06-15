using MultiShop.Basket.Dtos;
using MultiShop.Basket.Settings;
using System.Text.Json;

namespace MultiShop.Basket.Services
{
    public class BasketService : IBasketService
    {
        private static readonly JsonSerializerOptions RedisJson = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly RedisService _redisService;
        public BasketService(RedisService redisService)
        {
            _redisService = redisService;
        }
        public async Task DeleteBasket(string userId)
        {
            await _redisService.GetDb().KeyDeleteAsync(userId);
        }
        public async Task<BasketTotalDto> GetBasket(string userId)
        {
            var existBasket = await _redisService.GetDb().StringGetAsync(userId);
            if (existBasket.IsNullOrEmpty)
            {
                return new BasketTotalDto
                {
                    UserId = userId,
                    BasketItems = new List<BasketItemDto>()
                };
            }

            return JsonSerializer.Deserialize<BasketTotalDto>(existBasket!, RedisJson)
                   ?? new BasketTotalDto
                   {
                       UserId = userId,
                       BasketItems = new List<BasketItemDto>()
                   };
        }
        public async Task SaveBasket(BasketTotalDto basketTotalDto)
        {
            var payload = JsonSerializer.Serialize(basketTotalDto, RedisJson);
            await _redisService.GetDb().StringSetAsync(basketTotalDto.UserId, payload);
        }
    }
}