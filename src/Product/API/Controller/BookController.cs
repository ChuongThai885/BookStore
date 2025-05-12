using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private IBookService _service;
        public BookController(IBookService service)
        {
            this._service = service;
        }

        [HttpGet("books/getall")]
        public async Task<IEnumerable<BookDTO>> Get()
        {
            return await this._service.Get();
        }
        [HttpPost("books/create")]
        public async Task Add(BookCreateDTO dto)
        {
            await _service.Add(dto);
        }
    }
}
