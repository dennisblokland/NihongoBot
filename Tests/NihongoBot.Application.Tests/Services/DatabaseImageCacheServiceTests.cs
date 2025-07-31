using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NihongoBot.Application.Services;
using NihongoBot.Domain;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Infrastructure.Repositories;
using NihongoBot.Persistence;
using NihongoBot.Shared.Options;
using Xunit;

namespace NihongoBot.Application.Tests.Services;

public class DatabaseImageCacheServiceTests : IDisposable
{
	private readonly Mock<ILogger<DatabaseImageCacheService>> _loggerMock = new();
	private readonly DatabaseImageCacheService _imageCacheService;
	private readonly AppDbContext _context;
	private readonly IImageCacheRepository _repository;

	public DatabaseImageCacheServiceTests()
	{
		// Create in-memory database for testing
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new AppDbContext(options);
		
		// Create a mock service provider that returns our test context
		var serviceProviderMock = new Mock<IServiceProvider>();
		serviceProviderMock.Setup(x => x.GetService(typeof(AppDbContext))).Returns(_context);
		
		_repository = new ImageCacheRepository(serviceProviderMock.Object);

		var cacheOptions = new ImageCacheOptions
		{
			CacheExpirationHours = 1,
			EnableCleanup = true,
			CleanupIntervalHours = 1
		};
		
		var optionsMock = new Mock<IOptions<ImageCacheOptions>>();
		optionsMock.Setup(x => x.Value).Returns(cacheOptions);
		
		_imageCacheService = new DatabaseImageCacheService(_loggerMock.Object, optionsMock.Object, _repository);
	}

	public void Dispose()
	{
		_context.Dispose();
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
		
		// Wait a moment for the async operation to complete
		await Task.Delay(100);

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
	public async Task GetOrGenerateImageAsync_ShouldRegenerateExpiredImage()
	{
		// Arrange - Create service with very short expiration time for testing
		var shortExpirationOptions = new ImageCacheOptions
		{
			CacheExpirationHours = 0, // Immediate expiration for testing
			EnableCleanup = true,
			CleanupIntervalHours = 1
		};
		
		var optionsMock = new Mock<IOptions<ImageCacheOptions>>();
		optionsMock.Setup(x => x.Value).Returns(shortExpirationOptions);
		
		var shortExpirationService = new DatabaseImageCacheService(_loggerMock.Object, optionsMock.Object, _repository);
		
		string character = "と";
		
		// Create a cached entry  
		var cachedImage = new ImageCache(character, new byte[] { 1, 2, 3, 4 });
		await _repository.AddAsync(cachedImage, CancellationToken.None);
		await _repository.SaveChangesAsync();
		
		// Wait a tiny bit to ensure the entry is "expired" according to 0-hour expiration
		await Task.Delay(10);

		// Act
		byte[] newImageBytes = await shortExpirationService.GetOrGenerateImageAsync(character);

		// Assert
		Assert.NotEqual(new byte[] { 1, 2, 3, 4 }, newImageBytes);
		Assert.True(newImageBytes.Length > 4); // Real PNG should be larger
		
		var stats = shortExpirationService.GetCacheStats();
		Assert.Equal(0, stats.HitCount);
		Assert.Equal(1, stats.MissCount);
	}

	[Fact]
	public async Task CleanupExpiredFiles_ShouldRemoveExpiredEntries()
	{
		// Arrange - Create service with very short expiration time for testing
		var shortExpirationOptions = new ImageCacheOptions
		{
			CacheExpirationHours = 0, // Immediate expiration for testing
			EnableCleanup = true,
			CleanupIntervalHours = 1
		};
		
		var optionsMock = new Mock<IOptions<ImageCacheOptions>>();
		optionsMock.Setup(x => x.Value).Returns(shortExpirationOptions);
		
		var shortExpirationService = new DatabaseImageCacheService(_loggerMock.Object, optionsMock.Object, _repository);
		
		string character = "つ";
		
		// Create a cached entry
		var expiredImage = new ImageCache(character, new byte[] { 1, 2, 3, 4 });
		await _repository.AddAsync(expiredImage, CancellationToken.None);
		await _repository.SaveChangesAsync();
		
		// Wait a tiny bit to ensure the entry is "expired" according to 0-hour expiration
		await Task.Delay(10);

		// Act
		shortExpirationService.CleanupExpiredFiles();
		
		// Wait a moment for the async operation to complete
		await Task.Delay(100);

		// Assert
		var stats = shortExpirationService.GetCacheStats();
		Assert.Equal(0, stats.TotalEntries);
	}

	[Fact]
	public async Task Repository_ShouldStoreAndRetrieveImagesByCharacter()
	{
		// Arrange
		string character = "て";
		byte[] imageData = new byte[] { 1, 2, 3, 4, 5 };
		var imageCache = new ImageCache(character, imageData);

		// Act
		await _repository.AddAsync(imageCache, CancellationToken.None);
		await _repository.SaveChangesAsync();

		var retrieved = await _repository.GetByCharacterAsync(character);

		// Assert
		Assert.NotNull(retrieved);
		Assert.Equal(character, retrieved.Character);
		Assert.Equal(imageData, retrieved.ImageData);
		Assert.Equal(imageCache.CacheKey, retrieved.CacheKey);
	}

	[Fact]
	public async Task Repository_ShouldStoreAndRetrieveImagesByCacheKey()
	{
		// Arrange
		string character = "な";
		byte[] imageData = new byte[] { 5, 4, 3, 2, 1 };
		var imageCache = new ImageCache(character, imageData);

		// Act
		await _repository.AddAsync(imageCache, CancellationToken.None);
		await _repository.SaveChangesAsync();

		var retrieved = await _repository.GetByCacheKeyAsync(imageCache.CacheKey);

		// Assert
		Assert.NotNull(retrieved);
		Assert.Equal(character, retrieved.Character);
		Assert.Equal(imageData, retrieved.ImageData);
		Assert.Equal(imageCache.CacheKey, retrieved.CacheKey);
	}
}