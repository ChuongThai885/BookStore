using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Google.Apis.Auth;

namespace Authentication.Infrastructure.Security
{
    public class GoogleAuthValidator: IExternalAuthValidator
    {
        private readonly IConfiguration _config;
        public string ProviderName => "Google";
        public GoogleAuthValidator(IConfiguration config)
        {
            _config = config;
        }
        public async Task<ExternalUserPayload?> ValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _config["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
                return new ExternalUserPayload
                {
                    Provider = ProviderName,
                    ProviderKey = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    DisplayName = payload.Name
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
