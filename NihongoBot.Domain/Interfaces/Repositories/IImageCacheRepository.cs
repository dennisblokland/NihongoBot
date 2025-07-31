namespace NihongoBot.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for managing image cache entries in the database
/// </summary>
public interface IImageCacheRepository : IDomainRepository<ImageCache, Guid>
{
	/// <summary>
	/// Gets a cached image by cache key
	/// </summary>
	/// <param name="cacheKey">SHA256-based cache key</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>ImageCache entity if found, null otherwise</returns>
	Task<ImageCache?> GetByCacheKeyAsync(string cacheKey, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a cached image by character
	/// </summary>
	/// <param name="character">Japanese character</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>ImageCache entity if found, null otherwise</returns>
	Task<ImageCache?> GetByCharacterAsync(string character, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes expired cache entries based on the specified expiration hours
	/// </summary>
	/// <param name="expirationHours">Number of hours after which entries are considered expired</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Number of entries removed</returns>
	Task<int> RemoveExpiredAsync(int expirationHours, CancellationToken cancellationToken = default);

	/// <summary>
	/// Clears all cache entries
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Number of entries removed</returns>
	Task<int> ClearAllAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the total count of cached images
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Total number of cache entries</returns>
	Task<int> GetCountAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets a cached image by name
	/// </summary>
	/// <param name="name">The name of the cached image</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<ImageCache?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
