using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Product.Application.DTOs;
using Product.Application.DTOs.Common;
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

        public async Task<IEnumerable<AuthorDTO>> Get(QueryParams? queryParams)
        {
            var query = _repository.GetSet().AsNoTracking();

            if(!string.IsNullOrWhiteSpace(queryParams?.Search))
            {
                query = query.Where(item => item.Name.Contains(queryParams.Search.Trim()));
            }

            query = queryParams?.OrderBy switch
            {
                "name" => query.OrderBy(item => item.Name),
                "name_desc" => query.OrderByDescending(item => item.Name),
                _ => query.OrderBy(item => item.Name)
            };

            return await query
                .Skip(queryParams?.StartIndex ?? 0)
                .Take(queryParams?.PageSize ?? 10)
                .ProjectTo<AuthorDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
        public async Task<AuthorDTO?> GetAuthorById(Guid id)
        {
            var query = _repository.GetSet().AsNoTracking();

            return await query
                .Where(item => item.Id == id)
                .ProjectTo<AuthorDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
        public async Task Add(AuthorCreateDTO dto)
        {
            //dto.Name.IsNullOrWhiteSpace()
            var author = this._mapper.Map<AuthorEntity>(dto);
            await _repository.AddAsync(author);
        }
    }
}
