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
	public async Task WarmCacheAsync_ShouldThrowArgumentNullException_WhenCharactersIsNull()
	{
		// Act & Assert
		await Assert.ThrowsAsync<ArgumentNullException>(() => _imageCacheService.WarmCacheAsync(null!));
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
		Assert.Equal(character, retrieved.Name);
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
		Assert.Equal(character, retrieved.Name);
		Assert.Equal(imageData, retrieved.ImageData);
		Assert.Equal(imageCache.CacheKey, retrieved.CacheKey);
	}
}
