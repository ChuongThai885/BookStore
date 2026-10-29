namespace Authentication.Application.DTOs
{
    public class AuthDTO
    {
    }
    public record RegisterRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class ExternalLoginRequestDTO
    {
        public string Provider { get; set; } = string.Empty;
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
