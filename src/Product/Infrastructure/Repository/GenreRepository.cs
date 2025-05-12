using Product.Domain.Entity;
using Product.Domain.Interfaces;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Repository
{
    public class GenreRepository : BaseRepository<GenreEntity>, IGenreRepository
    {
        public GenreRepository(AppDbContext context) : base(context) { }
    }
}
