using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentication.Domain.Entity
{
    public class RefreshTokenEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid FamilyId { get; set; }
        [Required, MaxLength(128)]
        public string TokenHash { get; set; } = string.Empty;
        [MaxLength(128)]
        public string? ReplacedByTokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime AbsoluteExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
        [MaxLength(45)]
        public string? CreatedByIp { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUserEntity? User { get; set; }
    }
}
