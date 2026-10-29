using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Security
{
    public class MicrosoftAuthValidator: IExternalAuthValidator
    {
        private MicrosoftOptions _msOptions;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _oidcConfigManager;
        public string ProviderName => AuthConstants.ProviderNames.Microsoft;
        public MicrosoftAuthValidator(IOptions<ExternalAuthOptions> authOptions)
        {
            _msOptions = authOptions.Value.Microsoft;
            _oidcConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                _msOptions.MetadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true }
            );
        }
        public async Task<ExternalUserPayload?> ValidateAsync(string token, CancellationToken cancellationToken)
        {
            var clientId = _msOptions.ClientId;
            if(string.IsNullOrWhiteSpace(clientId))
                throw new InvalidOperationException("Microsoft ClientId is not configured.");

            var oidcConfig = await _oidcConfigManager.GetConfigurationAsync(cancellationToken);
            var handler = new JsonWebTokenHandler();

            var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudiences = [clientId],
                ValidateIssuer = true,
                IssuerValidator = (issuer, securityToken, validationParameters) =>
                {
                    if (issuer.StartsWith(_msOptions.ValidIssuerPrefix, StringComparison.OrdinalIgnoreCase) &&
                        issuer.EndsWith(_msOptions.ValidIssuerSuffix, StringComparison.OrdinalIgnoreCase))
                    {
                        return issuer;
                    }
                    throw new SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
                },
                ValidateLifetime = true,
                IssuerSigningKeys = oidcConfig.SigningKeys
            });

            if (!result.IsValid) return null;

            var claims = result.Claims;
            var providerKey = claims.TryGetValue(AuthConstants.Claims.MicrosoftObjectId, out var oid) ? oid?.ToString() 
                : claims.TryGetValue(AuthConstants.Claims.Subject, out var s) ? s?.ToString() 
                : null;
            var email = claims.TryGetValue(AuthConstants.Claims.Email, out var e) ? e?.ToString()
                      : claims.TryGetValue(AuthConstants.Claims.PreferredUsername, out var p) ? p?.ToString() : null;
            var name = claims.TryGetValue(AuthConstants.Claims.Name, out var n) ? n?.ToString() : email;

            if (string.IsNullOrEmpty(providerKey)) return null;

            return new ExternalUserPayload
            {
                Provider = ProviderName,
                ProviderKey = providerKey,  
                Email = email,
                EmailVerified = true,
                DisplayName = name
            };
        }
    }
}
