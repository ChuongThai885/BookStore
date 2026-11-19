using Cart.Application.DTO;

namespace Cart.Application.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItemDTO>> GetCart(Guid userId,CancellationToken cancellationToken);
        Task<long> AddToCart(Guid userId,CartItemDTO cartItem, CancellationToken cancellationToken);
        Task<long> RemoveFromCart(Guid userId, CartItemDTO cartItem, CancellationToken cancellationToken);
        Task ClearCart(Guid userId,CancellationToken cancellationToken);
    }
}
