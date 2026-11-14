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
    public class AuthTokenDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }
    public class LoginRequestDTO
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(8), MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
    public class ExternalLoginRequestDTO
    {
        [Required, MaxLength(32)]
        public string Provider { get; set; } = string.Empty;
        [Required, MaxLength(4096)]
        public string Token { get; set; } = string.Empty;
    }   
    public class ExternalUserPayload
    {
        public string Provider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public string? DisplayName { get; set; }
    }
}
