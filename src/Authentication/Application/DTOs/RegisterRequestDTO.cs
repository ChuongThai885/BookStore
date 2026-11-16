using System.ComponentModel.DataAnnotations;

namespace Authentication.Application.DTOs
{
    public record RegisterRequestDTO
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; set; } = string.Empty;
        [MaxLength(100)]
        public string? DisplayName { get; set; }
    }
}
