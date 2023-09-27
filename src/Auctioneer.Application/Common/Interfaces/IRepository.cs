namespace Auctioneer.Application.Common.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAsync();
    Task<TEntity?> GetAsync(Guid id);
    Task CreateAsync(TEntity newEntity);
    Task UpdateAsync(Guid id, TEntity updatedEntity);
    Task DeleteAsync(Guid id);
}