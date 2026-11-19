using BookStore.Common.Caching;
using Cart.Application.DTO;
using Cart.Application.Interfaces;
using Cart.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Cart.Application.Services
{
    public class CartService: ICartService
    {
        private readonly ICartRepository _repository;
        private readonly ILogger _logger;
        private readonly ICacheService _cacheService;
        public CartService(ICartRepository cartRepository, ILogger logger, ICacheService cacheService)
        {
            _repository = cartRepository;
            _logger = logger;
            _cacheService = cacheService;
        }
        private string GetKey (Guid id) => $"cart:{id}";
        public async Task<IEnumerable<CartItemDTO>> GetCart(Guid userId, CancellationToken cancellationToken)
        {
            var key = GetKey(userId);
            var hashData = await _cacheService.HashGetAllAsync(key);

            if(hashData != null && hashData.Count > 0)
            {
                return hashData.Select(kv => new CartItemDTO
                {
                    BookId = Guid.Parse(kv.Key),
                    Amount = (int)kv.Value, 
                });
            }

            var cartEntity = await _repository.GetSet(item => item.Id == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if(cartEntity != null && string.IsNullOrWhiteSpace(cartEntity.CartInfo))
            {
                var dbItems = JsonSerializer.Deserialize<Dictionary<string, int>>(cartEntity.CartInfo) ?? new();

                foreach (var item in dbItems)
                {
                    await _cacheService.HashIncrementAsync(key, item.Key, item.Value);
                }
                await _cacheService.KeyExpireAsync(key, TimeSpan.FromDays(7));

                return dbItems.Select(kv => new CartItemDTO
                {
                    BookId = Guid.Parse(kv.Key),
                    Amount = kv.Value
                });
            }

            return [];
        }
        public async Task AddToCart(Guid userId, CartItemDTO cartItem, CancellationToken cancellationToken)
        {
            var key = GetKey(userId);
            await _cacheService.HashIncrementAsync(key, cartItem.BookId.ToString(), cartItem.Amount);
            await _cacheService.KeyExpireAsync(key, TimeSpan.FromHours(24));
        }
        public async Task RemoveFromCart(Guid userId, CartItemDTO cartItem, CancellationToken cancellationToken)
        {
            var key = GetKey(userId);
            var newValue = await _cacheService.HashDecrementAsync(key, cartItem.BookId.ToString(), cartItem.Amount);

            if(newValue <= 0)
            {
                await _cacheService.HashDeleteAsync(key, cartItem.BookId.ToString());
                await _cacheService.KeyExpireAsync(key, TimeSpan.FromHours(24));
            }
        }
        public async Task ClearCart(Guid userId, CancellationToken cancellationToken)
        {
            var key = GetKey(userId);

            await _cacheService.RemoveAsync(key);
        }
    }
}
