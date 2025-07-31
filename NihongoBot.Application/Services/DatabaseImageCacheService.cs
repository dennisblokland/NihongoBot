using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NihongoBot.Application.Helpers;
using NihongoBot.Application.Interfaces;
using NihongoBot.Domain;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Shared.Options;

namespace NihongoBot.Application.Services;

/// <summary>
/// Database-based cache service for character images with thread-safe operations
/// </summary>
public class DatabaseImageCacheService : IImageCacheService
{
	private readonly ILogger<DatabaseImageCacheService> _logger;
	private readonly ImageCacheOptions _options;
	private readonly IImageCacheRepository _repository;
	private int _hitCount;
	private int _missCount;

	public DatabaseImageCacheService(ILogger<DatabaseImageCacheService> logger, IOptions<ImageCacheOptions> options, IImageCacheRepository repository)
	{
		_logger = logger;
		_options = options.Value;
		_repository = repository;
		_hitCount = 0;
		_missCount = 0;
	}

	public async Task<byte[]> GetOrGenerateImageAsync(string character)
	{
		if (string.IsNullOrWhiteSpace(character))
		{
			throw new ArgumentException("Character cannot be null or empty", nameof(character));
		}

		// Try to get from cache first
		ImageCache? cachedImage = await _repository.GetByCharacterAsync(character);

		if (cachedImage != null && !IsExpired(cachedImage))
		{
			Interlocked.Increment(ref _hitCount);
			_logger.LogDebug("Cache hit for character: {Character}", character);

			// Record access
			cachedImage.RecordAccess();
			_repository.Update(cachedImage);
			await _repository.SaveChangesAsync();

			return cachedImage.ImageData;
		}

		// Generate image if not in cache or expired
		Interlocked.Increment(ref _missCount);
		_logger.LogDebug("Cache miss for character: {Character}, generating image", character);

		byte[] imageBytes = await Task.Run(() => KanaRenderer.RenderCharacterToImage(character));

		// Cache the generated image to database
		if (cachedImage != null)
		{
			// Update existing expired entry
			cachedImage.UpdateImageData(imageBytes);
			cachedImage.RecordAccess();
			_repository.Update(cachedImage);
		}
		else
		{
			// Create new cache entry
			cachedImage = new ImageCache(character, imageBytes);
			cachedImage.RecordAccess();
			await _repository.AddAsync(cachedImage, CancellationToken.None);
		}

		await _repository.SaveChangesAsync();
		_logger.LogDebug("Cached image for character: {Character} in database", character);

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
		await Task.Run(async () =>
		{
			var tasks = characterList.Select(async character =>
			{
				if (cancellationToken.IsCancellationRequested)
					return;

				try
				{
					// Only generate if not already cached or expired
					ImageCache? cachedImage = await _repository.GetByCharacterAsync(character, cancellationToken);

					if (cachedImage == null || IsExpired(cachedImage))
					{
						byte[] imageBytes = KanaRenderer.RenderCharacterToImage(character);

						if (cachedImage != null)
						{
							// Update existing expired entry
							cachedImage.UpdateImageData(imageBytes);
							_repository.Update(cachedImage);
						}
						else
						{
							// Create new cache entry
							cachedImage = new ImageCache(character, imageBytes);
							await _repository.AddAsync(cachedImage, cancellationToken);
						}

						await _repository.SaveChangesAsync(cancellationToken);
						_logger.LogDebug("Pre-generated image for character: {Character}", character);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to pre-generate image for character: {Character}", character);
				}
			});

			await Task.WhenAll(tasks);
		}, cancellationToken);

		int totalCached = await _repository.GetCountAsync(cancellationToken);
		_logger.LogInformation("Cache warming completed. Total cached images: {Count}", totalCached);
	}

	public async Task ClearCacheAsync()
	{
		int removedCount = await _repository.ClearAllAsync();
		await _repository.SaveChangesAsync();

		Interlocked.Exchange(ref _hitCount, 0);
		Interlocked.Exchange(ref _missCount, 0);
		_logger.LogInformation("Cache cleared. Removed {Count} cached images", removedCount);
	}

	public async Task<(int HitCount, int MissCount, int TotalEntries)> GetCacheStatsAsync()
	{
		int totalEntries = await _repository.GetCountAsync();
		return (
			HitCount: _hitCount,
			MissCount: _missCount,
			TotalEntries: totalEntries
		);
	}

	public async Task CleanupExpiredFilesAsync()
	{
		int removedCount = await _repository.RemoveExpiredAsync(_options.CacheExpirationHours);
		if (removedCount > 0)
		{
			await _repository.SaveChangesAsync();
			_logger.LogInformation("Cleaned up {Count} expired cache entries", removedCount);
		}
	}

	/// <summary>
	/// Checks if a cache entry is expired based on the configured expiration time
	/// </summary>
	/// <param name="cachedImage">The cached image entry</param>
	/// <returns>True if the entry is expired, false otherwise</returns>
	private bool IsExpired(ImageCache cachedImage)
	{
		if (cachedImage.UpdatedAt == null)
		{
			return true; // Consider entries without update timestamp as expired
		}

		TimeSpan age = DateTime.UtcNow - cachedImage.UpdatedAt.Value;
		return age.TotalHours > _options.CacheExpirationHours;
	}

	public async Task<ImageCache> CacheAsync(string name, byte[] bytes)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Name cannot be null or empty", nameof(name));
		}

		if (bytes == null || bytes.Length == 0)
		{
			throw new ArgumentException("Image bytes cannot be null or empty", nameof(bytes));
		}

		ImageCache cachedImage = new(name, bytes);
		await _repository.AddAsync(cachedImage, CancellationToken.None);
		await _repository.SaveChangesAsync();
		return cachedImage;
	}

	public async Task<ImageCache?> TryGetAsync(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Name cannot be null or empty", nameof(name));
		}

		return await _repository.GetByNameAsync(name, CancellationToken.None);
	}

}
