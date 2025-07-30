namespace NihongoBot.Shared.Options;

public class ImageCacheOptions : IConfigOptions
{
	public const string SectionKey = "ImageCache";

	/// <summary>
	/// Directory path where cached images will be stored
	/// </summary>
	public string CacheDirectory { get; set; } = "cache/images";

	/// <summary>
	/// Maximum age of cached images in hours before they are considered expired
	/// </summary>
	public int CacheExpirationHours { get; set; } = 24 * 7; // 1 week default

	/// <summary>
	/// Whether to enable automatic cleanup of expired cache files
	/// </summary>
	public bool EnableCleanup { get; set; } = true;

	/// <summary>
	/// Interval in hours between automatic cleanup runs
	/// </summary>
	public int CleanupIntervalHours { get; set; } = 24; // Daily cleanup default
}