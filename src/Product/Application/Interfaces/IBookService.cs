using Product.Application.DTOs;

namespace Product.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDTO>> Get();
        Task Add(BookCreateDTO dto);
    }
}
