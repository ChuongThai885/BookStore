using Cart.Domain.Entity;
using Cart.Domain.Interfaces;
using Cart.Infrastructure.Data;

namespace Cart.Infrastructure.Repository
{
    public class CartRepository: BaseRepository<CartEntity>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context) { }
    }
}
