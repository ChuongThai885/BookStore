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
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IGenreRepository _genreRepository;
        public BookService(IBookRepository bookRepository, IAuthorRepository authorRepository, IGenreRepository genreRepository)
        {
            this._bookRepository = bookRepository;
            this._authorRepository = authorRepository;
            this._genreRepository = genreRepository;
        }
        public async Task<TableResponse<BookDTO>> Get(QueryParams? queryParams)
        {
            var query = _bookRepository.GetSet().AsNoTracking();

            if (!String.IsNullOrWhiteSpace(queryParams?.Search))
            {
                query = query.Where(item => item.Title.Contains(queryParams.Search.Trim()));
            }

            query = queryParams?.OrderBy?.Trim().ToLower() switch
            {
                "title" => query.OrderBy(b => b.Title),
                "title_desc" => query.OrderByDescending(b => b.Title),
                "price" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                _ => query.OrderBy(b => b.Title) // Default ordering by Title
            };

            int pageSize = queryParams?.PageSize ?? 10;
            int startIndex = (queryParams?.PageNumber - 1) * pageSize ?? 0;

            var totalRecords = await query.CountAsync();

            if(totalRecords == 0)
            {
                return new TableResponse<BookDTO>(
                    [],
                    totalRecords,
                    queryParams?.PageNumber ?? 1,
                    pageSize
                );
            }

            var data = await query
                .Skip(startIndex)
                .Take(pageSize)
                .Select(book => new BookDTO
                {
                    Id = book.Id,
                    Title = book.Title,
                    Price = book.Price,
                    Genres = book.Genres.Select(g => new BookGenreDTO { Id = g.Id, Name = g.Name }).ToList(),
                    Author =  new BookAuthorDTO { Id = book.Author.Id, Name = book.Author.Name }
                })
                .ToListAsync();

            return new TableResponse<BookDTO>(
                data,
                totalRecords,
                queryParams?.PageNumber ?? 1,
                pageSize
            );
        }
        public async Task<BookDTO?> Get(Guid id)
        {
            var query = _bookRepository.GetSet(item => item.Id == id).Select(book => new BookDTO { 
                Id = book.Id,
                Title = book.Title,
                Price = book.Price,
                Genres = book.Genres.Select(g => new BookGenreDTO { Id = g.Id, Name = g.Name }).ToList(),
                Author =  new BookAuthorDTO { Id = book.Author.Id, Name = book.Author.Name }
            });

            return await query.FirstOrDefaultAsync();
        }
        public async Task Add(BookCreateDTO dto)
        {
            var authorExists = await _authorRepository.GetSet().AnyAsync(a => a.Id == dto.AuthorId);

            if (!authorExists)
                throw new KeyNotFoundException($"Author with ID {dto.AuthorId} not found.");

            var genreIds = dto.GenreIds.Distinct().ToList();

            var genreList = await _genreRepository.GetSet()
                .Where(g => genreIds.Contains(g.Id))
                .ToListAsync();

            if (genreList.Count != genreIds.Count)
            {
                var missingGenres = genreIds.Except(genreList.Select(g => g.Id));
                throw new KeyNotFoundException($"Genres with IDs {string.Join(", ", missingGenres)} not found.");
            }

            var author = new AuthorEntity { Id = dto.AuthorId, Name = "" };

            _authorRepository.Attach(author);

            var entity = new BookEntity
            {
                Title = dto.Title,
                Author = author,
                Description = dto.Description,
                Genres = genreList,
                Quantity = dto.Quantity,
                Price = dto.Price
            };

            await _bookRepository.AddAsync(entity);
        }
    }
}
