
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NihongoBot.Domain.Interfaces;
using NihongoBot.Persistence;

namespace Name.Infrastructure.Repositories;

public abstract class AbstractDomainRepository<TEntity, TKey> : IDomainRepository<TEntity, TKey>
	where TEntity : class, IDomainEntity, IDomainEntity<TKey>
	where TKey : struct
{
	private readonly AppDbContext _databaseContext;

	protected DbSet<TEntity> DatabaseSet => _databaseContext.Set<TEntity>();


	protected AbstractDomainRepository(IServiceProvider serviceProvider)
	{
		_databaseContext = serviceProvider.GetRequiredService<AppDbContext>();
	}
	public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
	{
		await DatabaseSet.AddAsync(entity, cancellationToken);
		return entity;
	}

	public virtual TEntity Update(TEntity entity)
	{
		return DatabaseSet.Update(entity).Entity;
	}

	public virtual TEntity Remove(TEntity entity)
	{
		return DatabaseSet.Remove(entity).Entity;
	}

	public virtual async Task<TEntity?> FindByIdAsync(TKey id, CancellationToken cancellationToken)
	{
		return await DatabaseSet.FindAsync([id], cancellationToken);
	}

	public virtual async Task<List<TEntity>> FindByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken)
	{
		return await DatabaseSet
			.Where(entity => ids.Contains(entity.Id))
			.ToListAsync(cancellationToken);
	}

	public virtual async Task<IEnumerable<TEntity>> GetAsync(CancellationToken cancellationToken)
	{
		return await DatabaseSet.ToListAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await _databaseContext.SaveChangesAsync(cancellationToken);
	}

}
