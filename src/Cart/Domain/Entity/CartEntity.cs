using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cart.Domain.Entity
{
    [Table("Cart")]
    public class CartEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CartInfo { get; set; } = string.Empty;
    }
}
