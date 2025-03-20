namespace NihongoBot.Domain.Interfaces;

public interface IDomainRepository<TEntity, TKey> : IRepository
	where TEntity : class, IDomainEntity, IDomainEntity<TKey>
	where TKey : struct
{
		Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

		TEntity Update(TEntity entity);

		TEntity Remove(TEntity entity);

		Task<TEntity?> FindByIdAsync(
			TKey id, CancellationToken cancellationToken = default);

		Task<List<TEntity>> FindByIdsAsync(
			IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

		Task<IEnumerable<TEntity>> GetAsync(
			CancellationToken cancellationToken = default);

		Task SaveChangesAsync(
			CancellationToken cancellationToken = default);
}
