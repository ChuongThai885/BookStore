using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Domain.Entity
{
    [Table("Book")]
    public class BookEntity
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public required string Title { get; set; }
        public virtual AuthorEntity? Author { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<GenreEntity> Genre { get; set; } = [];
        public int Quantity { get; set; } = 0;
        public float Price { get; set; } = 0;
    }
}
