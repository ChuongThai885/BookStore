namespace Authentication.Application.DTOs
{
    public class ExternalUserPayload
    {
        public string Provider { get; set; } = string.Empty;
        public string ProviderKey { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public string? DisplayName { get; set; }
    }
}
