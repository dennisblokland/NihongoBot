using NihongoBot.Domain.Base;

namespace NihongoBot.Domain
{
	public class ImageCache : DomainEntity
	{
		internal ImageCache() { } // EF Core constructor

		public ImageCache(string name, byte[] imageData)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
			CacheKey = GenerateCacheKey(name);
		}

		/// <summary>
		/// The name of the cached image, typically the character it represents
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// SHA256-based cache key for efficient lookups
		/// </summary>
		public string CacheKey { get; private set; }

		/// <summary>
		/// PNG image data as byte array
		/// </summary>
		public byte[] ImageData { get; private set; }

		/// <summary>
		/// Number of times this cached image has been accessed
		/// </summary>
		public int AccessCount { get; private set; } = 0;

		/// <summary>
		/// Last time this cached image was accessed
		/// </summary>
		public DateTime? LastAccessedAt { get; private set; }

		/// <summary>
		/// Increments access count and updates last accessed timestamp
		/// </summary>
		public void RecordAccess()
		{
			AccessCount++;
			LastAccessedAt = DateTime.UtcNow;
		}

		/// <summary>
		/// Updates the image data for this cache entry
		/// </summary>
		/// <param name="imageData">New image data</param>
		public void UpdateImageData(byte[] imageData)
		{
			ImageData = imageData ?? throw new ArgumentNullException(nameof(imageData));
		}

		/// <summary>
		/// Generates a SHA256-based cache key for the character
		/// </summary>
		/// <param name="character">The character to generate key for</param>
		/// <returns>SHA256 hash as hex string (first 16 characters)</returns>
		private static string GenerateCacheKey(string character)
		{
			using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
			{
				byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(character));
				return Convert.ToHexString(hash)[..16]; // Use first 16 characters
			}
		}
	}
}
