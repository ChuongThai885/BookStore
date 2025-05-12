using Product.Domain.Entity;
using Product.Domain.Interfaces;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Repository
{
    public class BookRepository : BaseRepository<BookEntity>, IBookRepository
    {
        public BookRepository(AppDbContext context) : base(context) { }
    }
}
