using Microsoft.Extensions.Logging;
using NihongoBot.Application.Helpers;
using NihongoBot.Application.Interfaces;
using System.Collections.Concurrent;

namespace NihongoBot.Application.Services;

/// <summary>
/// In-memory cache service for character images with thread-safe operations
/// </summary>
public class ImageCacheService : IImageCacheService
{
	private readonly ILogger<ImageCacheService> _logger;
	private readonly ConcurrentDictionary<string, byte[]> _imageCache;
	private int _hitCount;
	private int _missCount;

	public ImageCacheService(ILogger<ImageCacheService> logger)
	{
		_logger = logger;
		_imageCache = new ConcurrentDictionary<string, byte[]>();
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
		if (_imageCache.TryGetValue(character, out byte[]? cachedImage))
		{
			Interlocked.Increment(ref _hitCount);
			_logger.LogDebug("Cache hit for character: {Character}", character);
			return cachedImage;
		}

		// Generate image if not in cache
		Interlocked.Increment(ref _missCount);
		_logger.LogDebug("Cache miss for character: {Character}, generating image", character);

		byte[] imageBytes = await Task.Run(() => KanaRenderer.RenderCharacterToImage(character));
		
		// Cache the generated image
		_imageCache.TryAdd(character, imageBytes);
		_logger.LogDebug("Cached image for character: {Character}, cache size: {CacheSize}", character, _imageCache.Count);

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
					// Only generate if not already cached
					if (!_imageCache.ContainsKey(character))
					{
						byte[] imageBytes = KanaRenderer.RenderCharacterToImage(character);
						_imageCache.TryAdd(character, imageBytes);
						_logger.LogDebug("Pre-generated image for character: {Character}", character);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to pre-generate image for character: {Character}", character);
				}
			});
		}, cancellationToken);

		_logger.LogInformation("Cache warming completed. Total cached images: {Count}", _imageCache.Count);
	}

	public void ClearCache()
	{
		int previousCount = _imageCache.Count;
		_imageCache.Clear();
		Interlocked.Exchange(ref _hitCount, 0);
		Interlocked.Exchange(ref _missCount, 0);
		_logger.LogInformation("Cache cleared. Removed {Count} cached images", previousCount);
	}

	public (int HitCount, int MissCount, int TotalEntries) GetCacheStats()
	{
		return (
			HitCount: _hitCount,
			MissCount: _missCount,
			TotalEntries: _imageCache.Count
		);
	}
}