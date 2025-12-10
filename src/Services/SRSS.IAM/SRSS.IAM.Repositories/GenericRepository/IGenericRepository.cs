using SRSS.IAM.Repositories.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.GenericRepository
{
    public interface IGenericRepository<TEntity, in TKey> where TEntity : BaseEntity<TKey>
    {
        Task<IEnumerable<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool isTracking = true,
        CancellationToken cancellationToken = default);

        Task<TEntity?> FindSingleAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool isTracking = true,
            CancellationToken cancellationToken = default);

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task RemoveMultipleAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}
