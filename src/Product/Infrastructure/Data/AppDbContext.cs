using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Product.Domain.Entity;

namespace Product.Infrastructure.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<AuthorEntity> Authors { get; set; }
        public DbSet<GenreEntity> Genres { get; set; }
    }
}
