using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Domain.Entity
{
    [Table("Genre")]
    public class GenreEntity
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<BookEntity> Books { get; set; } = [];
    }
}
