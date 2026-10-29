namespace Authentication.Domain.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
