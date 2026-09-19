using Product.Application.DTOs;
using Product.Application.DTOs.Common;

namespace Product.Application.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDTO>> Get(QueryParams? queryParams);
        Task<AuthorDTO?> GetAuthorById(Guid id);
        Task Add(AuthorCreateDTO data);
    }
}
