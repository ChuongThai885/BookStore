using Product.Application.DTOs;

namespace Product.Application.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDTO>> Get();
        Task Add(AuthorCreateDTO data);
    }
}
