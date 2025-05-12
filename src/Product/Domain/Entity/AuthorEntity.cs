using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.Domain.Entity
{
    [Table("Author")]
    public class AuthorEntity
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public virtual ICollection<BookEntity> Books { get; set; } = [];
    }
}
