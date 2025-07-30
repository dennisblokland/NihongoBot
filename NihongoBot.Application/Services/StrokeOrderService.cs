using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NihongoBot.Application.Interfaces;
using NihongoBot.Shared.Options;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace NihongoBot.Application.Services;

/// <summary>
/// Service for providing stroke order animations from Wikipedia with disk-based caching
/// </summary>
public class StrokeOrderService : IStrokeOrderService
{
	private readonly ILogger<StrokeOrderService> _logger;
	private readonly HttpClient _httpClient;
	private readonly ConcurrentDictionary<string, byte[]?> _animationCache;
	private readonly ImageCacheOptions _cacheOptions;
	private readonly string _strokeOrderCacheDirectory;
	private readonly SemaphoreSlim _diskCacheSemaphore;

	// Mapping of Hiragana characters to their Wikipedia stroke order animation URLs
	private readonly Dictionary<string, string> _strokeOrderUrls = new()
	{
		// Basic Hiragana
		{ "あ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%82_stroke_order_animation.gif" },
		{ "い", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%84_stroke_order_animation.gif" },
		{ "う", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%86_stroke_order_animation.gif" },
		{ "え", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%88_stroke_order_animation.gif" },
		{ "お", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%8A_stroke_order_animation.gif" },
		{ "か", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%8B_stroke_order_animation.gif" },
		{ "き", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%8D_stroke_order_animation.gif" },
		{ "く", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%8F_stroke_order_animation.gif" },
		{ "け", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%91_stroke_order_animation.gif" },
		{ "こ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%93_stroke_order_animation.gif" },
		{ "さ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%95_stroke_order_animation.gif" },
		{ "し", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%97_stroke_order_animation.gif" },
		{ "す", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%99_stroke_order_animation.gif" },
		{ "せ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%9B_stroke_order_animation.gif" },
		{ "そ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%9D_stroke_order_animation.gif" },
		{ "た", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%9F_stroke_order_animation.gif" },
		{ "ち", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%A1_stroke_order_animation.gif" },
		{ "つ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%A4_stroke_order_animation.gif" },
		{ "て", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%A6_stroke_order_animation.gif" },
		{ "と", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%A8_stroke_order_animation.gif" },
		{ "な", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%AA_stroke_order_animation.gif" },
		{ "に", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%AB_stroke_order_animation.gif" },
		{ "ぬ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%AC_stroke_order_animation.gif" },
		{ "ね", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%AD_stroke_order_animation.gif" },
		{ "の", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%AE_stroke_order_animation.gif" },
		{ "は", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%AF_stroke_order_animation.gif" },
		{ "ひ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%B2_stroke_order_animation.gif" },
		{ "ふ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%B5_stroke_order_animation.gif" },
		{ "へ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%B8_stroke_order_animation.gif" },
		{ "ほ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%BB_stroke_order_animation.gif" },
		{ "ま", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%BE_stroke_order_animation.gif" },
		{ "み", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%81%BF_stroke_order_animation.gif" },
		{ "む", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%80_stroke_order_animation.gif" },
		{ "め", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%81_stroke_order_animation.gif" },
		{ "も", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%82_stroke_order_animation.gif" },
		{ "や", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%84_stroke_order_animation.gif" },
		{ "ゆ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%86_stroke_order_animation.gif" },
		{ "よ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%88_stroke_order_animation.gif" },
		{ "ら", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%89_stroke_order_animation.gif" },
		{ "り", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%8A_stroke_order_animation.gif" },
		{ "る", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%8B_stroke_order_animation.gif" },
		{ "れ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%8C_stroke_order_animation.gif" },
		{ "ろ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%8D_stroke_order_animation.gif" },
		{ "わ", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%8F_stroke_order_animation.gif" },
		{ "を", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%92_stroke_order_animation.gif" },
		{ "ん", "https://en.wikipedia.org/wiki/Special:FilePath/Hiragana_%E3%82%93_stroke_order_animation.gif" }
	};

	public StrokeOrderService(ILogger<StrokeOrderService> logger, HttpClient httpClient, IOptions<ImageCacheOptions> cacheOptions)
	{
		_logger = logger;
		_httpClient = httpClient;
		_animationCache = new ConcurrentDictionary<string, byte[]?>();
		_cacheOptions = cacheOptions.Value;
		_diskCacheSemaphore = new SemaphoreSlim(1, 1);
		
		// Create stroke-order specific cache directory
		_strokeOrderCacheDirectory = Path.Combine(_cacheOptions.CacheDirectory, "stroke-order");
		Directory.CreateDirectory(_strokeOrderCacheDirectory);
		
		// Set a reasonable timeout for downloading animations
		_httpClient.Timeout = TimeSpan.FromSeconds(30);
	}

	public string? GetStrokeOrderAnimationUrl(string character)
	{
		if (string.IsNullOrWhiteSpace(character))
		{
			return null;
		}

		return _strokeOrderUrls.TryGetValue(character, out string? url) ? url : null;
	}

	public bool HasStrokeOrderAnimation(string character)
	{
		if (string.IsNullOrWhiteSpace(character))
		{
			return false;
		}

		return _strokeOrderUrls.ContainsKey(character);
	}

	public async Task<byte[]?> GetStrokeOrderAnimationBytesAsync(string character, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(character))
		{
			return null;
		}

		// Check in-memory cache first for best performance
		if (_animationCache.TryGetValue(character, out byte[]? cachedAnimation))
		{
			_logger.LogDebug("Stroke order animation in-memory cache hit for character: {Character}", character);
			return cachedAnimation;
		}

		// Check disk cache next
		string diskFileName = GetDiskCacheFileName(character);
		string diskFilePath = Path.Combine(_strokeOrderCacheDirectory, diskFileName);
		
		if (File.Exists(diskFilePath))
		{
			try
			{
				byte[] diskCachedBytes = await File.ReadAllBytesAsync(diskFilePath, cancellationToken);
				// Add to in-memory cache for faster future access
				_animationCache.TryAdd(character, diskCachedBytes);
				_logger.LogDebug("Stroke order animation disk cache hit for character: {Character}", character);
				return diskCachedBytes;
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to read cached stroke order animation from disk for character: {Character}", character);
				// Continue to download if disk read fails
			}
		}

		// Get URL for the character
		string? url = GetStrokeOrderAnimationUrl(character);
		if (url == null)
		{
			_logger.LogDebug("No stroke order animation available for character: {Character}", character);
			_animationCache.TryAdd(character, null);
			return null;
		}

		try
		{
			_logger.LogDebug("Downloading stroke order animation for character: {Character} from {Url}", character, url);
			
			byte[] animationBytes = await _httpClient.GetByteArrayAsync(url, cancellationToken);
			
			// Cache in memory
			_animationCache.TryAdd(character, animationBytes);
			
			// Cache to disk
			await CacheToDiskAsync(character, animationBytes, diskFilePath);
			
			_logger.LogDebug("Successfully downloaded and cached stroke order animation for character: {Character}, size: {Size} bytes", 
				character, animationBytes.Length);
			
			return animationBytes;
		}
		catch (HttpRequestException ex)
		{
			_logger.LogWarning(ex, "Failed to download stroke order animation for character: {Character} from {Url}", character, url);
			_animationCache.TryAdd(character, null);
			return null;
		}
		catch (TaskCanceledException ex)
		{
			_logger.LogWarning(ex, "Timeout downloading stroke order animation for character: {Character} from {Url}", character, url);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error downloading stroke order animation for character: {Character} from {Url}", character, url);
			_animationCache.TryAdd(character, null);
			return null;
		}
	}

	public IEnumerable<string> GetSupportedCharacters()
	{
		return _strokeOrderUrls.Keys;
	}

	/// <summary>
	/// Gets a safe filename for the character by hashing it, similar to ImageCacheService approach
	/// </summary>
	/// <param name="character">The character to get filename for</param>
	/// <returns>Safe filename with .gif extension</returns>
	private static string GetDiskCacheFileName(string character)
	{
		using (SHA256 sha256 = SHA256.Create())
		{
			byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"stroke_order_{character}"));
			string hashString = Convert.ToHexString(hash)[..16]; // Use first 16 characters
			return $"{hashString}.gif";
		}
	}

	/// <summary>
	/// Caches animation bytes to disk with thread safety
	/// </summary>
	/// <param name="character">The character being cached</param>
	/// <param name="animationBytes">The animation bytes to cache</param>
	/// <param name="filePath">The file path to save to</param>
	private async Task CacheToDiskAsync(string character, byte[] animationBytes, string filePath)
	{
		await _diskCacheSemaphore.WaitAsync();
		try
		{
			await File.WriteAllBytesAsync(filePath, animationBytes);
			_logger.LogDebug("Cached stroke order animation for character: {Character} to disk: {FilePath}", character, Path.GetFileName(filePath));
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to cache stroke order animation to disk for character: {Character}", character);
			// Don't rethrow - disk caching failure shouldn't break the main functionality
		}
		finally
		{
			_diskCacheSemaphore.Release();
		}
	}
}