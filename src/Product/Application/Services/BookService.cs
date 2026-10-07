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

            int startIndex = queryParams?.StartIndex ?? 0;
            int pageSize = queryParams?.PageSize ?? 10;

            var totalRecords = await query.CountAsync();

            if(totalRecords == 0)
            {
                return new TableResponse<BookDTO>
                {
                    Data = new List<BookDTO>(),
                    Total = 0,
                    CurrentPage = 1,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false
                };
            }

            var data = await query
                .Skip(startIndex)
                .Take(pageSize)
                .ProjectTo<BookDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new TableResponse<BookDTO>
            {
                Data = data,
                Total = totalRecords,
                CurrentPage = ((queryParams?.StartIndex ?? 0) / (queryParams?.PageSize ?? 10)) + 1,
                TotalPages = (int)Math.Ceiling((double)totalRecords / (queryParams?.PageSize ?? 10)),
                HasNextPage = (queryParams?.StartIndex ?? 0) + (queryParams?.PageSize ?? 10) < totalRecords,
                HasPreviousPage = (queryParams?.StartIndex ?? 0) > 0
            };
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
