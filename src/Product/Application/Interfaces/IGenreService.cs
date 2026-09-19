using Product.Application.DTOs;

namespace Product.Application.Interfaces
{
    public interface IGenreService
    {
        Task<GenreDTO?> GetGenreById(Guid id);
    }
}
