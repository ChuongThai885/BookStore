using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.DTOs.Common;
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

        [HttpPost("get")]
        public async Task<TableResponse<BookDTO>> Get(QueryParams? queryParams)
        {
            return await this._service.Get(queryParams);
        }
        [HttpPost("create")]
        public async Task Add(BookCreateDTO dto)
        {
            await _service.Add(dto);
        }
    }
}
