using System.ComponentModel.DataAnnotations;

namespace Authentication.Application.DTOs
{
    public class ExternalLoginRequestDTO
    {
        [Required, MaxLength(32)]
        public string Provider { get; set; } = string.Empty;
        [Required, MaxLength(4096)]
        public string Token { get; set; } = string.Empty;
        public bool? ConfirmLink { get; set; }
        public string? LinkPassword { get; set; }
    }
}
