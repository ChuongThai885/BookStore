namespace Product.Application.DTOs
{
    public class AuthorBookDTO
    {
        public Guid Id { get; set; } = new Guid();
        public required string Title { get; set; }
        public string? Description { get; set; }
        public ICollection<BookGenreDTO> Genre { get; set; } = [];
        public int Quantity { get; set; } = 0;
        public float Price { get; set; } = 0;
    }
    public class AuthorDTO
    {
        public Guid Id { get; set; } = new Guid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<AuthorBookDTO> Books { get; set; } = [];
    }
    public class AuthorCreateDTO
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
