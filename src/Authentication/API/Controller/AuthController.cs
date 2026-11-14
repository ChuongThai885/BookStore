using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Domain.Constants;
using Authentication.Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace Authentication.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        private readonly JwtOptions _jwtOptions;
        public AuthController(IAuthServices authServices, IOptions<JwtOptions> jwtOptions)
        {
            _authServices = authServices;
            _jwtOptions = jwtOptions.Value;
        }
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken ct)
        {
            var result = await _authServices.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new { result.UserId, result.Email, result.DisplayName });
        }
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct)
        {
            var result = await _authServices.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new { result.UserId, result.Email, result.DisplayName });
        }
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequestDTO request, CancellationToken cancellationToken)
        {
            var result = await _authServices.ExternalLoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new { result.UserId, result.Email, result.DisplayName });
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            if(!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
            {
                return Unauthorized(new { message = "Refresh token is missing." });
            }
            
            var result = await _authServices.RefreshTokenAsync(refreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new { result.UserId, result.Email, result.DisplayName });
        }
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            Request.Cookies.TryGetValue(AuthConstants.Cookies.RefreshToken, out var refreshToken);
            var jti = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            await _authServices.LogoutAsync(refreshToken, jti, cancellationToken);

            Response.Cookies.Delete(AuthConstants.Cookies.AccessToken, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = AuthConstants.Cookies.RootPath 
            });
            Response.Cookies.Delete(AuthConstants.Cookies.RefreshToken, new CookieOptions {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = AuthConstants.Cookies.AuthPath 
            });

            return Ok(new { message = "Logged out successfully." });
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append(AuthConstants.Cookies.AccessToken, accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = AuthConstants.Cookies.RootPath,
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes)
            });

            Response.Cookies.Append(AuthConstants.Cookies.RefreshToken, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = AuthConstants.Cookies.AuthPath,
                Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            });
        }
    }
}
