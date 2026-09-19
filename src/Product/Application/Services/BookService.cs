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
        private readonly IMapper _mapper;
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        public BookService(IMapper mapper, IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            this._mapper = mapper;
            this._bookRepository = bookRepository;
            this._authorRepository = authorRepository;
        }
        public async Task<IEnumerable<BookDTO>> Get(QueryParams? queryParams)
        {
            var query = _bookRepository.GetSet().AsNoTracking();

            if(!String.IsNullOrWhiteSpace(queryParams?.Search))
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

            return await query
                .Skip(queryParams?.StartIndex ?? 0)
                .Take(queryParams?.PageSize ?? 10)
                .ProjectTo<BookDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }
        public async Task Add(BookCreateDTO dto)
        {
            if (dto.AuthorId == null)
                throw new ArgumentException("AuthorId is required.");

            var author = await _authorRepository.FindAsync(item => item.Id == dto.AuthorId);

            if (author == null)
                throw new KeyNotFoundException($"Author with ID {dto.AuthorId} not found.");

            var entity = this._mapper.Map<BookEntity>(dto);

            entity.Author = author;

            author.Books.Add(entity);

            await _bookRepository.AddAsync(entity);
        }
    }
}
