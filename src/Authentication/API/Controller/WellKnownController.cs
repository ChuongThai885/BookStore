using Authentication.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controller
{
    [ApiController]
    [Route(".well-known")]
    public class WellKnownController : ControllerBase
    {
        private readonly JwtKeyManager _keyManager;
        private readonly IConfiguration _config;
        public WellKnownController(JwtKeyManager keyManager, IConfiguration config)
        {
            _keyManager = keyManager;
            _config = config;
        }
        [HttpGet("openid-configuration")]
        public IActionResult GetOpenIdConfiguration()
        {
            var issuer = _config["Jwt:Issuer"] ?? $"{Request.Scheme}://{Request.Host}";
            return Ok(new
            {
                issuer = issuer,
                jwks_uri = $"{issuer}/.well-known/jwks.json",
                id_token_signing_alg_values_supported = new[] { "ES256" }
            });
        }
        [HttpGet("jwks.json")]
        public IActionResult GetJwks()
        {
            return Ok(new
            {
                keys = new[] { _keyManager.PublicJwk }
            });
        }
    }
}
