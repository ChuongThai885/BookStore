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
        private readonly IAuthorRepository _repository;
        public AuthorService(IAuthorRepository _repository)
        {
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

            int pageSize = queryParams?.PageSize ?? 10;
            int startIndex = (queryParams?.PageNumber - 1) * pageSize ?? 0;

            return await query
                .Skip(startIndex)
                .Take(pageSize)
                .Select(item => new AuthorDTO
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Books = item.Books.Select(book => new AuthorBookDTO
                    {
                        Id = book.Id,
                        Title = book.Title,
                        Description = book.Description,
                        Price = book.Price,
                        Quantity = book.Quantity,
                        Genres = book.Genres.Select(genre => new BookGenreDTO
                        {
                            Id = genre.Id,
                            Name = genre.Name
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();
        }
        public async Task<AuthorDTO?> GetAuthorById(Guid id)
        {
            var query = _repository.GetSet().AsNoTracking();

            return await query
                .Where(item => item.Id == id)
                .Select(item => new AuthorDTO
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Books = item.Books.Select(book => new AuthorBookDTO
                    {
                        Id = book.Id,
                        Title = book.Title,
                        Description = book.Description,
                        Price = book.Price,
                        Quantity = book.Quantity,
                        Genres = book.Genres.Select(genre => new BookGenreDTO
                        {
                            Id = genre.Id,
                            Name = genre.Name
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }
        public async Task Add(AuthorCreateDTO dto)
        {
            var author = new AuthorEntity
            {
                Name = dto.Name,
                Description = dto.Description
            };
            await _repository.AddAsync(author);
        }
    }
}
