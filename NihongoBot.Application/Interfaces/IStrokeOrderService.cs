namespace NihongoBot.Application.Interfaces;

/// <summary>
/// Service for providing stroke order animations for Japanese characters
/// </summary>
public interface IStrokeOrderService
{
	/// <summary>
	/// Gets the stroke order animation URL for a given character
	/// </summary>
	/// <param name="character">The character to get stroke order for</param>
	/// <returns>URL to the stroke order animation GIF, or null if not available</returns>
	string? GetStrokeOrderAnimationUrl(string character);

	/// <summary>
	/// Checks if stroke order animation is available for a character
	/// </summary>
	/// <param name="character">The character to check</param>
	/// <returns>True if stroke order animation is available</returns>
	bool HasStrokeOrderAnimation(string character);

	/// <summary>
	/// Downloads and caches stroke order animation bytes for a character
	/// </summary>
	/// <param name="character">The character to download stroke order for</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Animated GIF bytes, or null if not available</returns>
	Task<byte[]?> GetStrokeOrderAnimationBytesAsync(string character, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets all characters that have stroke order animations available
	/// </summary>
	/// <returns>List of characters with stroke order animations</returns>
	IEnumerable<string> GetSupportedCharacters();
}