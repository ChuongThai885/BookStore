namespace Authentication.Application.DTOs
{
    public class AuthTokenDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public bool RequireLinking { get; set; } = false;
        public bool isExistingEmailConfirmed { get; set; } = false;
        public string? Message { get; set; }
    }
}
