using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Product.Application.DTOs;
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
        public async Task<IEnumerable<BookDTO>> Get()
        {
            var query = await _bookRepository.GetSet().Include(item => item.Author).ToListAsync();
            var result = this._mapper.Map<IEnumerable<BookDTO>>(query); // Fix: Map to IEnumerable<BookDTO>
            return result;
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
