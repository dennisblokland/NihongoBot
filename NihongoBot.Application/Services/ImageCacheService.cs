using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NihongoBot.Application.Helpers;
using NihongoBot.Application.Interfaces;
using NihongoBot.Shared.Options;
using System.Security.Cryptography;
using System.Text;

namespace NihongoBot.Application.Services;

/// <summary>
/// Disk-based cache service for character images with thread-safe operations
/// </summary>
public class ImageCacheService : IImageCacheService
{
	private readonly ILogger<ImageCacheService> _logger;
	private readonly ImageCacheOptions _options;
	private readonly SemaphoreSlim _semaphore;
	private int _hitCount;
	private int _missCount;

	public ImageCacheService(ILogger<ImageCacheService> logger, IOptions<ImageCacheOptions> options)
	{
		_logger = logger;
		_options = options.Value;
		_semaphore = new SemaphoreSlim(1, 1);
		_hitCount = 0;
		_missCount = 0;

		// Ensure cache directory exists
		Directory.CreateDirectory(_options.CacheDirectory);
	}

	public async Task<byte[]> GetOrGenerateImageAsync(string character)
	{
		if (string.IsNullOrWhiteSpace(character))
		{
			throw new ArgumentException("Character cannot be null or empty", nameof(character));
		}

		string fileName = GetCacheFileName(character);
		string filePath = Path.Combine(_options.CacheDirectory, fileName);

		// Try to get from cache first
		if (File.Exists(filePath) && !IsFileExpired(filePath))
		{
			Interlocked.Increment(ref _hitCount);
			_logger.LogDebug("Cache hit for character: {Character}", character);
			return await File.ReadAllBytesAsync(filePath);
		}

		// Generate image if not in cache or expired
		Interlocked.Increment(ref _missCount);
		_logger.LogDebug("Cache miss for character: {Character}, generating image", character);

		byte[] imageBytes = await Task.Run(() => KanaRenderer.RenderCharacterToImage(character));
		
		// Cache the generated image to disk
		await _semaphore.WaitAsync();
		try
		{
			await File.WriteAllBytesAsync(filePath, imageBytes);
			_logger.LogDebug("Cached image for character: {Character} to file: {FilePath}", character, fileName);
		}
		finally
		{
			_semaphore.Release();
		}

		return imageBytes;
	}

	public async Task WarmCacheAsync(IEnumerable<string> characters, CancellationToken cancellationToken = default)
	{
		if (characters == null)
		{
			throw new ArgumentNullException(nameof(characters));
		}

		List<string> characterList = characters.ToList();
		_logger.LogInformation("Warming cache for {Count} characters", characterList.Count);

		// Use parallel processing for better performance during cache warming
		await Task.Run(() =>
		{
			Parallel.ForEach(characterList, new ParallelOptions
			{
				CancellationToken = cancellationToken,
				MaxDegreeOfParallelism = Environment.ProcessorCount
			}, character =>
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				try
				{
					string fileName = GetCacheFileName(character);
					string filePath = Path.Combine(_options.CacheDirectory, fileName);

					// Only generate if not already cached or expired
					if (!File.Exists(filePath) || IsFileExpired(filePath))
					{
						byte[] imageBytes = KanaRenderer.RenderCharacterToImage(character);
						File.WriteAllBytes(filePath, imageBytes);
						_logger.LogDebug("Pre-generated image for character: {Character}", character);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to pre-generate image for character: {Character}", character);
				}
			});
		}, cancellationToken);

		_logger.LogInformation("Cache warming completed. Total cached images: {Count}", GetCachedFileCount());
	}

	public void ClearCache()
	{
		int previousCount = GetCachedFileCount();
		
		if (Directory.Exists(_options.CacheDirectory))
		{
			foreach (string file in Directory.GetFiles(_options.CacheDirectory, "*.png"))
			{
				try
				{
					File.Delete(file);
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to delete cache file: {FilePath}", file);
				}
			}
		}

		Interlocked.Exchange(ref _hitCount, 0);
		Interlocked.Exchange(ref _missCount, 0);
		_logger.LogInformation("Cache cleared. Removed {Count} cached images", previousCount);
	}

	public (int HitCount, int MissCount, int TotalEntries) GetCacheStats()
	{
		return (
			HitCount: _hitCount,
			MissCount: _missCount,
			TotalEntries: GetCachedFileCount()
		);
	}

	/// <summary>
	/// Gets a safe filename for the character by hashing it
	/// </summary>
	/// <param name="character">The character to get filename for</param>
	/// <returns>Safe filename with .png extension</returns>
	private static string GetCacheFileName(string character)
	{
		using (SHA256 sha256 = SHA256.Create())
		{
			byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(character));
			string hashString = Convert.ToHexString(hash)[..16]; // Use first 16 characters
			return $"{hashString}.png";
		}
	}

	/// <summary>
	/// Checks if a cache file is expired based on the configured expiration time
	/// </summary>
	/// <param name="filePath">Path to the cache file</param>
	/// <returns>True if the file is expired, false otherwise</returns>
	private bool IsFileExpired(string filePath)
	{
		try
		{
			DateTime lastWriteTime = File.GetLastWriteTime(filePath);
			TimeSpan age = DateTime.UtcNow - lastWriteTime.ToUniversalTime();
			return age.TotalHours > _options.CacheExpirationHours;
		}
		catch
		{
			// If we can't determine the age, consider it expired
			return true;
		}
	}

	/// <summary>
	/// Gets the count of cached PNG files in the cache directory
	/// </summary>
	/// <returns>Number of cached files</returns>
	private int GetCachedFileCount()
	{
		try
		{
			if (!Directory.Exists(_options.CacheDirectory))
				return 0;

			return Directory.GetFiles(_options.CacheDirectory, "*.png").Length;
		}
		catch
		{
			return 0;
		}
	}

	/// <summary>
	/// Cleans up expired cache files
	/// </summary>
	public void CleanupExpiredFiles()
	{
		if (!_options.EnableCleanup || !Directory.Exists(_options.CacheDirectory))
			return;

		int removedCount = 0;
		try
		{
			foreach (string file in Directory.GetFiles(_options.CacheDirectory, "*.png"))
			{
				if (IsFileExpired(file))
				{
					try
					{
						File.Delete(file);
						removedCount++;
					}
					catch (Exception ex)
					{
						_logger.LogWarning(ex, "Failed to delete expired cache file: {FilePath}", file);
					}
				}
			}

			if (removedCount > 0)
			{
				_logger.LogInformation("Cleaned up {Count} expired cache files", removedCount);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during cache cleanup");
		}
	}
}