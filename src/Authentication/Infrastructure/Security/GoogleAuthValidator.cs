using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Options;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Authentication.Infrastructure.Security
{
    public class GoogleAuthValidator: IExternalAuthValidator
    {
        private readonly GoogleOptions _googleOptions;
        public string ProviderName => AuthConstants.ProviderNames.Google;
        public GoogleAuthValidator(IOptions<ExternalAuthOptions> googleOptions)
        {
            _googleOptions = googleOptions.Value.Google;
        }
        public async Task<ExternalUserPayload?> ValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                var clientId = _googleOptions.ClientId;
                if (string.IsNullOrWhiteSpace(clientId))
                    throw new InvalidOperationException("Google ClientId is not configured.");

                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = [clientId]
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
