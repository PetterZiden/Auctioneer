namespace Auctioneer.Application.Common.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAsync();
    Task<(int totalPages, List<TEntity> data)> GetAsync(int page, int pageSize);
    Task<TEntity?> GetAsync(Guid id);
    Task CreateAsync(TEntity newEntity, CancellationToken cancellationToken);
    Task UpdateAsync(Guid id, TEntity updatedEntity, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}