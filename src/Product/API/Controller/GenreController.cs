using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController: ControllerBase
    {
        private IGenreService _service;
        public GenreController(IGenreService service)
        {
            this._service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<GenreDTO>>> GetGenreById(Guid id)
        {
            var genres = await _service.GetGenreById(id);
            return Ok(genres);
        }
    }
}
