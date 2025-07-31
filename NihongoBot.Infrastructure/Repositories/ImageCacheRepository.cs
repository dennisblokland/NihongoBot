using Microsoft.EntityFrameworkCore;
using Name.Infrastructure.Repositories;
using NihongoBot.Domain;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class ImageCacheRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<ImageCache, Guid>(serviceProvider), IImageCacheRepository
{
	public async Task<ImageCache?> GetByCacheKeyAsync(string cacheKey, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.CacheKey == cacheKey, cancellationToken);
	}

	public async Task<ImageCache?> GetByCharacterAsync(string character, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.Character == character, cancellationToken);
	}

	public async Task<int> RemoveExpiredAsync(int expirationHours, CancellationToken cancellationToken = default)
	{
		DateTime expirationTime = DateTime.UtcNow.AddHours(-expirationHours);
		
		List<ImageCache> expiredEntries = await DatabaseSet
			.Where(x => x.UpdatedAt != null && x.UpdatedAt < expirationTime)
			.ToListAsync(cancellationToken);

		if (expiredEntries.Count > 0)
		{
			DatabaseSet.RemoveRange(expiredEntries);
		}

		return expiredEntries.Count;
	}

	public async Task<int> ClearAllAsync(CancellationToken cancellationToken = default)
	{
		List<ImageCache> allEntries = await DatabaseSet.ToListAsync(cancellationToken);
		
		if (allEntries.Count > 0)
		{
			DatabaseSet.RemoveRange(allEntries);
		}

		return allEntries.Count;
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.CountAsync(cancellationToken);
	}
}