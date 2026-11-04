using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Entity;
using Authentication.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Infrastructure.Security
{
    public class TokenServices: ITokenServices
    {
        private readonly JwtKeyManager _keyManager;
        private readonly JwtOptions _jwtOptions;
        public TokenServices(JwtKeyManager keyManager, IOptions<JwtOptions> jwtOptions)
        {
            _keyManager = keyManager;
            _jwtOptions = jwtOptions.Value;
        }
        public string GenerateAccessToken(ApplicationUserEntity user, IList<string> roles)
        {
            var handler = new JsonWebTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                SigningCredentials = new SigningCredentials(_keyManager.PrivateKey, SecurityAlgorithms.EcdsaSha256),
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                    [JwtRegisteredClaimNames.Email] = user.Email ?? string.Empty,
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                    [AuthConstants.Claims.DisplayName] = user.DisplayName ?? string.Empty,
                    [AuthConstants.Claims.Role] = roles
                }
            };
            return handler.CreateToken(descriptor);
        }
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
        public string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes);
        }
    }
}
