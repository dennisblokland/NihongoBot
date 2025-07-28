using Microsoft.Extensions.Logging;
using NihongoBot.Application.Interfaces;
using System.Collections.Concurrent;
using System.Text;

namespace NihongoBot.Application.Services;

/// <summary>
/// Service for providing stroke order animations from Wikimedia Commons
/// </summary>
public class StrokeOrderService : IStrokeOrderService
{
	private readonly ILogger<StrokeOrderService> _logger;
	private readonly HttpClient _httpClient;
	private readonly ConcurrentDictionary<string, byte[]?> _animationCache;

	// Mapping of Hiragana characters to their Wikimedia Commons stroke order animation URLs
	private readonly Dictionary<string, string> _strokeOrderUrls = new()
	{
		// Basic Hiragana
		{ "あ", "https://upload.wikimedia.org/wikipedia/commons/0/00/Hiragana_%E3%81%82_stroke_order_animation.gif" },
		{ "い", "https://upload.wikimedia.org/wikipedia/commons/d/d0/Hiragana_%E3%81%84_stroke_order_animation.gif" },
		{ "う", "https://upload.wikimedia.org/wikipedia/commons/7/7f/Hiragana_%E3%81%86_stroke_order_animation.gif" },
		{ "え", "https://upload.wikimedia.org/wikipedia/commons/b/bf/Hiragana_%E3%81%88_stroke_order_animation.gif" },
		{ "お", "https://upload.wikimedia.org/wikipedia/commons/9/9a/Hiragana_%E3%81%8A_stroke_order_animation.gif" },
		{ "か", "https://upload.wikimedia.org/wikipedia/commons/f/fc/Hiragana_%E3%81%8B_stroke_order_animation.gif" },
		{ "き", "https://upload.wikimedia.org/wikipedia/commons/6/6c/Hiragana_%E3%81%8D_stroke_order_animation.gif" },
		{ "く", "https://upload.wikimedia.org/wikipedia/commons/1/11/Hiragana_%E3%81%8F_stroke_order_animation.gif" },
		{ "け", "https://upload.wikimedia.org/wikipedia/commons/3/3c/Hiragana_%E3%81%91_stroke_order_animation.gif" },
		{ "こ", "https://upload.wikimedia.org/wikipedia/commons/8/81/Hiragana_%E3%81%93_stroke_order_animation.gif" },
		{ "さ", "https://upload.wikimedia.org/wikipedia/commons/3/35/Hiragana_%E3%81%95_stroke_order_animation.gif" },
		{ "し", "https://upload.wikimedia.org/wikipedia/commons/c/ca/Hiragana_%E3%81%97_stroke_order_animation.gif" },
		{ "す", "https://upload.wikimedia.org/wikipedia/commons/a/aa/Hiragana_%E3%81%99_stroke_order_animation.gif" },
		{ "せ", "https://upload.wikimedia.org/wikipedia/commons/5/51/Hiragana_%E3%81%9B_stroke_order_animation.gif" },
		{ "そ", "https://upload.wikimedia.org/wikipedia/commons/3/3f/Hiragana_%E3%81%9D_stroke_order_animation.gif" },
		{ "た", "https://upload.wikimedia.org/wikipedia/commons/5/5b/Hiragana_%E3%81%9F_stroke_order_animation.gif" },
		{ "ち", "https://upload.wikimedia.org/wikipedia/commons/4/4f/Hiragana_%E3%81%A1_stroke_order_animation.gif" },
		{ "つ", "https://upload.wikimedia.org/wikipedia/commons/4/49/Hiragana_%E3%81%A4_stroke_order_animation.gif" },
		{ "て", "https://upload.wikimedia.org/wikipedia/commons/f/f2/Hiragana_%E3%81%A6_stroke_order_animation.gif" },
		{ "と", "https://upload.wikimedia.org/wikipedia/commons/d/dc/Hiragana_%E3%81%A8_stroke_order_animation.gif" },
		{ "な", "https://upload.wikimedia.org/wikipedia/commons/2/26/Hiragana_%E3%81%AA_stroke_order_animation.gif" },
		{ "に", "https://upload.wikimedia.org/wikipedia/commons/b/b3/Hiragana_%E3%81%AB_stroke_order_animation.gif" },
		{ "ぬ", "https://upload.wikimedia.org/wikipedia/commons/4/41/Hiragana_%E3%81%AC_stroke_order_animation.gif" },
		{ "ね", "https://upload.wikimedia.org/wikipedia/commons/1/1b/Hiragana_%E3%81%AD_stroke_order_animation.gif" },
		{ "の", "https://upload.wikimedia.org/wikipedia/commons/a/a2/Hiragana_%E3%81%AE_stroke_order_animation.gif" },
		{ "は", "https://upload.wikimedia.org/wikipedia/commons/a/a5/Hiragana_%E3%81%AF_stroke_order_animation.gif" },
		{ "ひ", "https://upload.wikimedia.org/wikipedia/commons/a/a2/Hiragana_%E3%81%B2_stroke_order_animation.gif" },
		{ "ふ", "https://upload.wikimedia.org/wikipedia/commons/f/f7/Hiragana_%E3%81%B5_stroke_order_animation.gif" },
		{ "へ", "https://upload.wikimedia.org/wikipedia/commons/c/c5/Hiragana_%E3%81%B8_stroke_order_animation.gif" },
		{ "ほ", "https://upload.wikimedia.org/wikipedia/commons/d/da/Hiragana_%E3%81%BB_stroke_order_animation.gif" },
		{ "ま", "https://upload.wikimedia.org/wikipedia/commons/0/04/Hiragana_%E3%81%BE_stroke_order_animation.gif" },
		{ "み", "https://upload.wikimedia.org/wikipedia/commons/b/be/Hiragana_%E3%81%BF_stroke_order_animation.gif" },
		{ "む", "https://upload.wikimedia.org/wikipedia/commons/3/30/Hiragana_%E3%82%80_stroke_order_animation.gif" },
		{ "め", "https://upload.wikimedia.org/wikipedia/commons/5/50/Hiragana_%E3%82%81_stroke_order_animation.gif" },
		{ "も", "https://upload.wikimedia.org/wikipedia/commons/5/5b/Hiragana_%E3%82%82_stroke_order_animation.gif" },
		{ "や", "https://upload.wikimedia.org/wikipedia/commons/c/cb/Hiragana_%E3%82%84_stroke_order_animation.gif" },
		{ "ゆ", "https://upload.wikimedia.org/wikipedia/commons/9/98/Hiragana_%E3%82%86_stroke_order_animation.gif" },
		{ "よ", "https://upload.wikimedia.org/wikipedia/commons/3/3e/Hiragana_%E3%82%88_stroke_order_animation.gif" },
		{ "ら", "https://upload.wikimedia.org/wikipedia/commons/8/8e/Hiragana_%E3%82%89_stroke_order_animation.gif" },
		{ "り", "https://upload.wikimedia.org/wikipedia/commons/7/7c/Hiragana_%E3%82%8A_stroke_order_animation.gif" },
		{ "る", "https://upload.wikimedia.org/wikipedia/commons/2/27/Hiragana_%E3%82%8B_stroke_order_animation.gif" },
		{ "れ", "https://upload.wikimedia.org/wikipedia/commons/4/48/Hiragana_%E3%82%8C_stroke_order_animation.gif" },
		{ "ろ", "https://upload.wikimedia.org/wikipedia/commons/9/9a/Hiragana_%E3%82%8D_stroke_order_animation.gif" },
		{ "わ", "https://upload.wikimedia.org/wikipedia/commons/e/e6/Hiragana_%E3%82%8F_stroke_order_animation.gif" },
		{ "を", "https://upload.wikimedia.org/wikipedia/commons/c/c0/Hiragana_%E3%82%92_stroke_order_animation.gif" },
		{ "ん", "https://upload.wikimedia.org/wikipedia/commons/c/c4/Hiragana_%E3%82%93_stroke_order_animation.gif" }
	};

	public StrokeOrderService(ILogger<StrokeOrderService> logger, HttpClient httpClient)
	{
		_logger = logger;
		_httpClient = httpClient;
		_animationCache = new ConcurrentDictionary<string, byte[]?>();
		
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

		// Check cache first
		if (_animationCache.TryGetValue(character, out byte[]? cachedAnimation))
		{
			_logger.LogDebug("Stroke order animation cache hit for character: {Character}", character);
			return cachedAnimation;
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
			
			// Cache the result
			_animationCache.TryAdd(character, animationBytes);
			
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
}