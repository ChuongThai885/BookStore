using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Security
{
    public class MicrosoftAuthValidator: IExternalAuthValidator
    {
        private readonly IConfiguration _config;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _oidcConfigManager;
        public string ProviderName => "Microsoft";
        public MicrosoftAuthValidator(IConfiguration config)
        {
            _config = config;
            _oidcConfigManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever()
            );
        }
        public async Task<ExternalUserPayload?> ValidateAsync(string token, CancellationToken cancellationToken)
        {
            var oidcConfig = await _oidcConfigManager.GetConfigurationAsync(cancellationToken);
            var handler = new JsonWebTokenHandler();

            var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudiences = new[] { _config["Authentication:Microsoft:ClientId"] },
                ValidateIssuer = false,
                ValidateLifetime = true,
                IssuerSigningKeys = oidcConfig.SigningKeys
            });

            if (!result.IsValid) return null;

            var claims = result.Claims;
            var providerKey = claims.TryGetValue("oid", out var oid) ? oid?.ToString() 
                : claims.TryGetValue("sub", out var s) ? s?.ToString() 
                : null;
            var email = claims.TryGetValue("email", out var e) ? e?.ToString()
                      : claims.TryGetValue("preferred_username", out var p) ? p?.ToString() : null;
            var name = claims.TryGetValue("name", out var n) ? n?.ToString() : email;

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
