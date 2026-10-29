using Authentication.Application.Interfaces;
using Authentication.Domain.Entity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Infrastructure.Security
{
    public class TokenServices: ITokenServices
    {
        private readonly JwtKeyManager _keyManager;
        private readonly IConfiguration _config;
        public TokenServices(JwtKeyManager keyManager, IConfiguration config)
        {
            _keyManager = keyManager;
            _config = config;
        }
        public string GenerateAccessToken(ApplicationUserEntity user, IList<string> roles)
        {
            var handler = new JsonWebTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _config["Jwt:Issuer"] ?? "https://localhost:7001",
                Audience = _config["Jwt:Audience"] ?? "bookstore-microservices",
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(_keyManager.PrivateKey, SecurityAlgorithms.EcdsaSha256),
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                    [JwtRegisteredClaimNames.Email] = user.Email ?? string.Empty,
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                    ["display_name"] = user.DisplayName ?? string.Empty,
                    ["role"] = roles
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
