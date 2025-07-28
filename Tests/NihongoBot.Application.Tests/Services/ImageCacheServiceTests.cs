using Microsoft.Extensions.Logging;
using Moq;
using NihongoBot.Application.Services;
using Xunit;

namespace NihongoBot.Application.Tests.Services;

public class ImageCacheServiceTests
{
	private readonly Mock<ILogger<ImageCacheService>> _loggerMock = new();
	private readonly ImageCacheService _imageCacheService;

	public ImageCacheServiceTests()
	{
		_imageCacheService = new ImageCacheService(_loggerMock.Object);
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
}