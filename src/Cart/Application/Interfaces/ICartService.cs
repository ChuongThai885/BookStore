using Cart.Application.DTO;

namespace Cart.Application.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDTO>> GetCart(Guid userId,CancellationToken cancellationToken = default);
        Task AddToCart(Guid userId,CartItemDTO cartItem, CancellationToken cancellationToken = default);
        Task RemoveFromCart(Guid userId, CartItemDTO cartItem, CancellationToken cancellationToken = default);
        Task ClearCart(Guid userId, CancellationToken cancellationToken = default);
    }
}
