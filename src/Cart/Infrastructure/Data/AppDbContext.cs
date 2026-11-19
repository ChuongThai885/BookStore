using Cart.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Cart.Infrastructure.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<CartEntity> Carts { get; set; }
    }
}
