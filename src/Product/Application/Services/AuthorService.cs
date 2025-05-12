using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Entity;
using Product.Domain.Interfaces;

namespace Product.Application.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IMapper _mapper;
        private readonly IAuthorRepository _repository;
        public AuthorService(IMapper mapper, IAuthorRepository _repository)
        {
            this._mapper = mapper;
            this._repository = _repository;
        }

        public async Task<IEnumerable<AuthorDTO>> Get()
        {
            var query = await _repository.GetSet().Include(item => item.Books).ToListAsync();
            var result = this._mapper.Map<IEnumerable<AuthorDTO>>(query);
            return result;
        }
        public async Task Add(AuthorCreateDTO dto)
        {
            //dto.Name.IsNullOrWhiteSpace()
            var author = this._mapper.Map<AuthorEntity>(dto);
            await _repository.AddAsync(author);
        }
    }
}
