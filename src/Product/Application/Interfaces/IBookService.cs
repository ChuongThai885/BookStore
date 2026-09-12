using Product.Application.DTOs;
using Product.Application.DTOs.Common;

namespace Product.Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDTO>> Get(QueryParams? queryParams);
        Task Add(BookCreateDTO dto);
    }
}
