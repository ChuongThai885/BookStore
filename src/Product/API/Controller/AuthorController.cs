using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _services;
        public AuthorController(IAuthorService _services)
        {
            this._services = _services;
        }
        [HttpGet("author/getall")]
        public async Task<IEnumerable<AuthorDTO>> Get()
        {
            return await _services.Get();
        }
        [HttpPost("author/add")]
        public async Task Add(AuthorCreateDTO author)
        {
            await _services.Add(author);
        }

    }
}
