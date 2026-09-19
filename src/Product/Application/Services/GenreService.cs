using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Interfaces;

namespace Product.Application.Services
{
    public class GenreService: IGenreService
    {
        private readonly IMapper _mapper;
        private readonly IGenreRepository _repository;
        public GenreService(IMapper mapper, IGenreRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }
        public async Task<GenreDTO?> GetGenreById(Guid id)
        {
            //var query = _repository.GetSet().AsNoTracking().Include(item => item.Books).FirstOrDefaultAsync(g => g.Id == id);

            //var result = this._mapper.Map<GenreDTO>(await query);

            //return result;
            var query = _repository.GetSet().AsNoTracking();

            
            return await query
                .Where(item => item.Id == id)
                .ProjectTo<GenreDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}
