namespace NihongoBot.Application.Interfaces;

/// <summary>
/// Service for caching generated character images to improve performance
/// </summary>
public interface IImageCacheService
{
	/// <summary>
	/// Gets a cached image for the specified character, or generates and caches it if not found
	/// </summary>
	/// <param name="character">The character to generate/retrieve image for</param>
	/// <returns>PNG image bytes</returns>
	Task<byte[]> GetOrGenerateImageAsync(string character);

	/// <summary>
	/// Pre-generates and caches images for all provided characters
	/// </summary>
	/// <param name="characters">Characters to pre-generate images for</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task WarmCacheAsync(IEnumerable<string> characters, CancellationToken cancellationToken = default);

	/// <summary>
	/// Clears the image cache
	/// </summary>
	void ClearCache();

	/// <summary>
	/// Gets cache statistics
	/// </summary>
	/// <returns>Cache statistics (hit count, miss count, total entries)</returns>
	(int HitCount, int MissCount, int TotalEntries) GetCacheStats();

	/// <summary>
	/// Cleans up expired cache files
	/// </summary>
	void CleanupExpiredFiles();
}