using Product.Domain.Entity;

namespace Product.Application.DTOs
{
    public class GenreBookDTO
    {
        public Guid Id { get; set; } = new Guid();
        public required string Title { get; set; }
        public BookAuthorDTO? Author { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; } = 0;
        public float Price { get; set; } = 0;
    }
    public class GenreDTO
    {
        public Guid Id { get; set; } = new Guid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<BookEntity> Books { get; set; } = [];
    }
}
