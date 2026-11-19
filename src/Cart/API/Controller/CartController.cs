using Cart.Application.DTO;
using Cart.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cart.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _services;
        public CartController(ICartService services)
        {
            _services = services;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDTO cartItem)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized(new { message = "Invalid token or missing user identifier." });
            }

            await _services.AddToCart(userId, cartItem);
            return Ok(new { message = "Item added to cart." });
        }
    }
}
