using Product.Domain.Entity;
using Product.Infrastructure.Interface;

namespace Product.Domain.Interfaces
{
    public interface IAuthorRepository: IBaseRepository<AuthorEntity>
    {
    }
}
