using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.DTOs.Common;
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
        [HttpPost("author/get")]
        public async Task<IEnumerable<AuthorDTO>> Get(QueryParams? queryParams)
        {
            return await _services.Get(queryParams);
        }
        [HttpGet("author/{id}")]
        public async Task<AuthorDTO?> GetAuthorById(Guid id)
        {
            return await _services.GetAuthorById(id);
        }
        [HttpPost("author/add")]
        public async Task Add(AuthorCreateDTO author)
        {
            await _services.Add(author);
        }

    }
}
