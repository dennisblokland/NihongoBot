using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NihongoBot.Application.Interfaces;
using NihongoBot.Application.Services;
using NihongoBot.Shared.Options;
using Xunit;

namespace NihongoBot.Application.Tests.Services;

public class ImageCacheServiceTests : IDisposable
{
	private readonly Mock<ILogger<ImageCacheService>> _loggerMock = new();
	private readonly ImageCacheService _imageCacheService;
	private readonly string _testCacheDirectory;

	public ImageCacheServiceTests()
	{
		// Create a unique test cache directory
		_testCacheDirectory = Path.Combine(Path.GetTempPath(), $"test_cache_{Guid.NewGuid():N}");
		
		var options = new ImageCacheOptions
		{
			CacheDirectory = _testCacheDirectory,
			CacheExpirationHours = 1,
			EnableCleanup = true,
			CleanupIntervalHours = 1
		};
		
		var optionsMock = new Mock<IOptions<ImageCacheOptions>>();
		optionsMock.Setup(x => x.Value).Returns(options);
		
		_imageCacheService = new ImageCacheService(_loggerMock.Object, optionsMock.Object);
	}

	public void Dispose()
	{
		// Clean up test cache directory
		if (Directory.Exists(_testCacheDirectory))
		{
			Directory.Delete(_testCacheDirectory, true);
		}
	}

	[Fact]
	public async Task GetOrGenerateImageAsync_ShouldGenerateImage_WhenCharacterIsNotCached()
	{
		// Arrange
		string character = "あ";

		// Act
		byte[] imageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);

		// Assert
		Assert.NotNull(imageBytes);
		Assert.True(imageBytes.Length > 0);

		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(0, stats.HitCount);
		Assert.Equal(1, stats.MissCount);
		Assert.Equal(1, stats.TotalEntries);
	}

	[Fact]
	public async Task GetOrGenerateImageAsync_ShouldReturnCachedImage_WhenCharacterIsAlreadyCached()
	{
		// Arrange
		string character = "か";

		// First call to generate and cache the image
		byte[] firstImageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);

		// Act - Second call should return cached image
		byte[] secondImageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);

		// Assert
		Assert.Equal(firstImageBytes, secondImageBytes);

		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(1, stats.HitCount);
		Assert.Equal(1, stats.MissCount);
		Assert.Equal(1, stats.TotalEntries);
	}

	[Fact]
	public async Task GetOrGenerateImageAsync_ShouldThrowArgumentException_WhenCharacterIsNullOrEmpty()
	{
		// Act & Assert
		await Assert.ThrowsAsync<ArgumentException>(() => _imageCacheService.GetOrGenerateImageAsync(null!));
		await Assert.ThrowsAsync<ArgumentException>(() => _imageCacheService.GetOrGenerateImageAsync(""));
		await Assert.ThrowsAsync<ArgumentException>(() => _imageCacheService.GetOrGenerateImageAsync(" "));
	}

	[Fact]
	public async Task WarmCacheAsync_ShouldCacheAllProvidedCharacters()
	{
		// Arrange
		List<string> characters = new() { "き", "く", "け", "こ" };

		// Act
		await _imageCacheService.WarmCacheAsync(characters);

		// Assert
		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(characters.Count, stats.TotalEntries);
		Assert.Equal(0, stats.HitCount); // No hits during warmup
		Assert.Equal(0, stats.MissCount); // Direct cache insertion, no misses

		// Verify each character is cached by requesting it
		foreach (string character in characters)
		{
			byte[] imageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);
			Assert.NotNull(imageBytes);
			Assert.True(imageBytes.Length > 0);
		}

		// All requests should be hits now
		var finalStats = _imageCacheService.GetCacheStats();
		Assert.Equal(characters.Count, finalStats.HitCount);
	}

	[Fact]
	public async Task WarmCacheAsync_ShouldThrowArgumentNullException_WhenCharactersIsNull()
	{
		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _imageCacheService.WarmCacheAsync(null!));
	}

	[Fact]
	public async Task ClearCache_ShouldRemoveAllCachedImagesAndResetStats()
	{
		// Arrange - Add some items to cache
		await _imageCacheService.GetOrGenerateImageAsync("さ");
		await _imageCacheService.GetOrGenerateImageAsync("し");

		var statsBeforeClear = _imageCacheService.GetCacheStats();
		Assert.Equal(2, statsBeforeClear.TotalEntries);

		// Act
		_imageCacheService.ClearCache();

		// Assert
		var statsAfterClear = _imageCacheService.GetCacheStats();
		Assert.Equal(0, statsAfterClear.HitCount);
		Assert.Equal(0, statsAfterClear.MissCount);
		Assert.Equal(0, statsAfterClear.TotalEntries);
	}

	[Fact]
	public async Task GetCacheStats_ShouldReturnCorrectStatistics()
	{
		// Arrange & Act
		await _imageCacheService.GetOrGenerateImageAsync("す"); // Miss
		await _imageCacheService.GetOrGenerateImageAsync("せ"); // Miss
		await _imageCacheService.GetOrGenerateImageAsync("す"); // Hit (same character)

		// Assert
		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(1, stats.HitCount);
		Assert.Equal(2, stats.MissCount);
		Assert.Equal(2, stats.TotalEntries);
	}

	[Fact]
	public async Task ConcurrentAccess_ShouldBeThreadSafe()
	{
		// Arrange
		string character = "そ";
		int numberOfTasks = 10;

		// Act - Multiple concurrent requests for the same character
		List<Task<byte[]>> tasks = new();
		for (int i = 0; i < numberOfTasks; i++)
		{
			tasks.Add(_imageCacheService.GetOrGenerateImageAsync(character));
		}

		byte[][] results = await Task.WhenAll(tasks);

		// Assert - All results should be identical (same cached image)
		for (int i = 1; i < results.Length; i++)
		{
			Assert.Equal(results[0], results[i]);
		}

		// Should only have one entry in cache
		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(1, stats.TotalEntries);
		// Note: In concurrent scenarios, the exact hit count can vary depending on timing,
		// but we should have at least some hits and total requests should equal numberOfTasks
		Assert.True(stats.HitCount + stats.MissCount >= numberOfTasks);
	}

	[Fact]
	public async Task GetOrGenerateImageAsync_ShouldUseCachedFile_WhenFileExists()
	{
		// Arrange
		string character = "た";
		
		// First call to generate and cache the image
		byte[] firstImageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);
		
		// Verify file was created
		string[] cacheFiles = Directory.GetFiles(_testCacheDirectory, "*.png");
		Assert.Single(cacheFiles);

		// Act - Second call should return cached image from disk
		byte[] secondImageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);

		// Assert
		Assert.Equal(firstImageBytes, secondImageBytes);
		
		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(1, stats.HitCount);
		Assert.Equal(1, stats.MissCount);
		Assert.Equal(1, stats.TotalEntries);
	}

	[Fact]
	public void CleanupExpiredFiles_ShouldRemoveExpiredFiles()
	{
		// Arrange
		string character = "つ";
		string fileName = GetCacheFileNameForCharacter(character);
		string filePath = Path.Combine(_testCacheDirectory, fileName);
		
		// Create a test file with old timestamp
		byte[] testBytes = [1, 2, 3, 4];
		File.WriteAllBytes(filePath, testBytes);
		
		// Set file timestamp to be older than expiration (2 hours ago)
		DateTime oldTime = DateTime.UtcNow.AddHours(-2);
		File.SetLastWriteTimeUtc(filePath, oldTime);

		// Act
		_imageCacheService.CleanupExpiredFiles();

		// Assert
		Assert.False(File.Exists(filePath));
	}

	[Fact]
	public void CleanupExpiredFiles_ShouldKeepNonExpiredFiles()
	{
		// Arrange
		string character = "て";
		string fileName = GetCacheFileNameForCharacter(character);
		string filePath = Path.Combine(_testCacheDirectory, fileName);
		
		// Create a test file with recent timestamp
		byte[] testBytes = [1, 2, 3, 4];
		File.WriteAllBytes(filePath, testBytes);

		// Act
		_imageCacheService.CleanupExpiredFiles();

		// Assert
		Assert.True(File.Exists(filePath));
	}

	[Fact]
	public async Task GetOrGenerateImageAsync_ShouldRegenerateExpiredImage()
	{
		// Arrange
		string character = "と";
		string fileName = GetCacheFileNameForCharacter(character);
		string filePath = Path.Combine(_testCacheDirectory, fileName);
		
		// Create an expired cached file
		byte[] oldBytes = [1, 2, 3, 4]; // Different from what KanaRenderer would generate
		File.WriteAllBytes(filePath, oldBytes);
		DateTime oldTime = DateTime.UtcNow.AddHours(-2);
		File.SetLastWriteTimeUtc(filePath, oldTime);

		// Act
		byte[] newImageBytes = await _imageCacheService.GetOrGenerateImageAsync(character);

		// Assert
		Assert.NotEqual(oldBytes, newImageBytes);
		Assert.True(newImageBytes.Length > oldBytes.Length); // Real PNG should be larger
		
		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(0, stats.HitCount);
		Assert.Equal(1, stats.MissCount);
	}

	[Fact]
	public void GetCacheStats_ShouldReturnCorrectFileCount()
	{
		// Arrange - Create some test cache files
		for (int i = 0; i < 3; i++)
		{
			string fileName = $"test_{i}.png";
			string filePath = Path.Combine(_testCacheDirectory, fileName);
			File.WriteAllBytes(filePath, [1, 2, 3]);
		}

		// Act
		var stats = _imageCacheService.GetCacheStats();

		// Assert
		Assert.Equal(3, stats.TotalEntries);
	}

	[Fact]
	public void ClearCache_ShouldDeleteAllCacheFiles()
	{
		// Arrange - Create some test cache files
		for (int i = 0; i < 3; i++)
		{
			string fileName = $"test_{i}.png";
			string filePath = Path.Combine(_testCacheDirectory, fileName);
			File.WriteAllBytes(filePath, [1, 2, 3]);
		}

		Assert.Equal(3, Directory.GetFiles(_testCacheDirectory, "*.png").Length);

		// Act
		_imageCacheService.ClearCache();

		// Assert
		Assert.Empty(Directory.GetFiles(_testCacheDirectory, "*.png"));
		
		var stats = _imageCacheService.GetCacheStats();
		Assert.Equal(0, stats.TotalEntries);
		Assert.Equal(0, stats.HitCount);
		Assert.Equal(0, stats.MissCount);
	}

	[Fact]
	public void CleanupExpiredFiles_ShouldBeCallableFromInterface()
	{
		// Arrange
		IImageCacheService cacheService = _imageCacheService;
		
		string character = "な";
		string fileName = GetCacheFileNameForCharacter(character);
		string filePath = Path.Combine(_testCacheDirectory, fileName);
		
		// Create a test file with old timestamp
		byte[] testBytes = [1, 2, 3, 4];
		File.WriteAllBytes(filePath, testBytes);
		
		// Set file timestamp to be older than expiration (2 hours ago)
		DateTime oldTime = DateTime.UtcNow.AddHours(-2);
		File.SetLastWriteTimeUtc(filePath, oldTime);

		// Act - Should be callable through interface
		cacheService.CleanupExpiredFiles();

		// Assert
		Assert.False(File.Exists(filePath));
	}

	/// <summary>
	/// Helper method to get the expected cache filename for a character
	/// This duplicates the logic from ImageCacheService.GetCacheFileName for testing
	/// </summary>
	private static string GetCacheFileNameForCharacter(string character)
	{
		using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
		{
			byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(character));
			string hashString = Convert.ToHexString(hash)[..16];
			return $"{hashString}.png";
		}
	}
}