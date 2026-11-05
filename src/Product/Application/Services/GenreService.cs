using Microsoft.EntityFrameworkCore;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Interfaces;

namespace Product.Application.Services
{
    public class GenreService: IGenreService
    {
        private readonly IGenreRepository _repository;
        public GenreService(IGenreRepository repository)
        {
            _repository = repository;
        }
        public async Task<GenreDTO?> GetGenreById(Guid id)
        {
            var query = _repository.GetSet().AsNoTracking();

            
            return await query
                .Where(item => item.Id == id)
                .Select(item => new GenreDTO
                {
                    Id = item.Id,
                    Name = item.Name
                })
                .FirstOrDefaultAsync();
        }
    }
}
