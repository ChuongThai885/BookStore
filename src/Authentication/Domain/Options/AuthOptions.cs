using System.ComponentModel.DataAnnotations;

namespace Authentication.Domain.Options
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";
        [Required] public string Issuer { get; init; } = string.Empty;
        [Required] public string Audience { get; init; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; init; } = 15;
        public int RefreshTokenExpirationDays { get; init; } = 7;
        public int AbsoluteRefreshTokenExpirationDays { get; init; } = 30;
        public int KeyRotationDays { get; init; } = 30;
        public int KeyGracePeriodDays { get; init; } = 2;
        public int CleanTokenDurationHours { get; init; } = 24;
    }
    public class ExternalAuthOptions
    {
        public const string SectionName = "Authentication";
        public GoogleOptions Google { get; init; } = new();
        public MicrosoftOptions Microsoft { get; init; } = new();
    }
    public class GoogleOptions
    {
        [Required] public string ClientId { get; init; } = string.Empty;
    }
    public class MicrosoftOptions
    {
        [Required] public string ClientId { get; init; } = string.Empty;
        [Required]
        public string MetadataAddress { get; init; } =
            "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";
        [Required]
        public string ValidIssuerPrefix { get; init; } =
            "https://login.microsoftonline.com/";
        [Required] public string ValidIssuerSuffix { get; init; } = "/v2.0";
    }
}
