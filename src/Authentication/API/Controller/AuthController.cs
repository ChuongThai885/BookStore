using Authentication.Application.DTOs;
using Authentication.Application.Interfaces;
using Authentication.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request, CancellationToken ct)
        {
            var result = await _authServices.RegisterAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new { result.UserId, result.Email, result.DisplayName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request, CancellationToken ct)
        {
            var result = await _authServices.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(new { result.UserId, result.Email, result.DisplayName });
        }
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
        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth",
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }
    }
}
