using NihongoBot.Domain;

namespace NihongoBot.Application.Interfaces;

/// <summary>
/// Service for caching generated character images to improve performance
/// </summary>
public interface IImageCacheService
{
	/// <summary>
	/// Manually caches an image by name
	/// </summary>
	/// <param name="name"></param>
	/// <param name="bytes"></param>
	/// <returns></returns>
	Task<ImageCache> Cache(string name, byte[] bytes);

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
	/// Tries to get a cached image by name
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	Task<ImageCache?> TryGetAsync(string name);
}
