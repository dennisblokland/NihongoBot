using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NihongoBot.Application.Services;
using NihongoBot.Shared.Options;
using System.Net;
using Xunit;

namespace NihongoBot.Application.Tests.Services;

public class StrokeOrderServiceTests
{
	private readonly Mock<ILogger<StrokeOrderService>> _loggerMock = new();
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
	private readonly Mock<IOptions<ImageCacheOptions>> _cacheOptionsMock = new();
	private readonly HttpClient _httpClient;
	private readonly StrokeOrderService _strokeOrderService;

	public StrokeOrderServiceTests()
	{
		_httpClient = new HttpClient(_httpMessageHandlerMock.Object);
		
		// Setup mock cache options
		_cacheOptionsMock.Setup(x => x.Value).Returns(new ImageCacheOptions
		{
			CacheDirectory = Path.Combine(Path.GetTempPath(), "test-cache", Guid.NewGuid().ToString()),
			CacheExpirationHours = 168, // 1 week
			EnableCleanup = true,
			CleanupIntervalHours = 24
		});
		
		_strokeOrderService = new StrokeOrderService(_loggerMock.Object, _httpClient, _cacheOptionsMock.Object);
	}

	[Fact]
	public void GetStrokeOrderAnimationUrl_ShouldReturnCorrectUrl_ForValidCharacter()
	{
		// Arrange
		string character = "あ";

		// Act
		string? url = _strokeOrderService.GetStrokeOrderAnimationUrl(character);

		// Assert
		Assert.NotNull(url);
		Assert.Contains("Hiragana_%E3%81%82_stroke_order_animation.gif", url);
		Assert.StartsWith("https://en.wikipedia.org/wiki/Special:FilePath/", url);
	}

	[Fact]
	public void GetStrokeOrderAnimationUrl_ShouldReturnNull_ForInvalidCharacter()
	{
		// Arrange
		string character = "z"; // Not a supported character

		// Act
		string? url = _strokeOrderService.GetStrokeOrderAnimationUrl(character);

		// Assert
		Assert.Null(url);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	public void GetStrokeOrderAnimationUrl_ShouldReturnNull_ForNullOrEmptyCharacter(string? character)
	{
		// Act
		string? url = _strokeOrderService.GetStrokeOrderAnimationUrl(character!);

		// Assert
		Assert.Null(url);
	}

	[Fact]
	public void HasStrokeOrderAnimation_ShouldReturnTrue_ForSupportedCharacter()
	{
		// Arrange
		string character = "か";

		// Act
		bool hasAnimation = _strokeOrderService.HasStrokeOrderAnimation(character);

		// Assert
		Assert.True(hasAnimation);
	}

	[Fact]
	public void HasStrokeOrderAnimation_ShouldReturnFalse_ForUnsupportedCharacter()
	{
		// Arrange
		string character = "x"; // Not a supported character

		// Act
		bool hasAnimation = _strokeOrderService.HasStrokeOrderAnimation(character);

		// Assert
		Assert.False(hasAnimation);
	}

	[Fact]
	public async Task GetStrokeOrderAnimationBytesAsync_ShouldReturnBytes_WhenDownloadSucceeds()
	{
		// Arrange
		string character = "き";
		byte[] expectedBytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF header

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new ByteArrayContent(expectedBytes)
			});

		// Act
		byte[]? result = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedBytes, result);
	}

	[Fact]
	public async Task GetStrokeOrderAnimationBytesAsync_ShouldReturnNull_ForUnsupportedCharacter()
	{
		// Arrange
		string character = "y"; // Not a supported character

		// Act
		byte[]? result = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetStrokeOrderAnimationBytesAsync_ShouldReturnNull_WhenHttpRequestFails()
	{
		// Arrange
		string character = "く";

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.NotFound
			});

		// Act
		byte[]? result = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetStrokeOrderAnimationBytesAsync_ShouldCacheResult()
	{
		// Arrange
		string character = "け";
		byte[] expectedBytes = new byte[] { 0x47, 0x49, 0x46 };

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new ByteArrayContent(expectedBytes)
			});

		// Act - First call
		byte[]? result1 = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Act - Second call
		byte[]? result2 = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Assert
		Assert.Equal(result1, result2);
		
		// Verify HTTP was called only once (cached on second call)
		_httpMessageHandlerMock
			.Protected()
			.Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>());
	}

	[Fact]
	public void GetSupportedCharacters_ShouldReturnNonEmptyCollection()
	{
		// Act
		IEnumerable<string> supportedCharacters = _strokeOrderService.GetSupportedCharacters();

		// Assert
		Assert.NotEmpty(supportedCharacters);
		Assert.Contains("あ", supportedCharacters);
		Assert.Contains("ん", supportedCharacters);
		// Should contain basic hiragana
		Assert.True(supportedCharacters.Count() >= 40); // At least 40 hiragana characters
	}

	[Fact]
	public async Task GetStrokeOrderAnimationBytesAsync_ShouldCacheToDisk_WhenDownloadSucceeds()
	{
		// Arrange
		string character = "き";
		byte[] expectedBytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF header

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new ByteArrayContent(expectedBytes)
			});

		// Act - First call should download and cache
		byte[]? result = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(expectedBytes, result);

		// Verify that a cache file was created in the stroke-order subdirectory
		string cacheDirectory = _cacheOptionsMock.Object.Value.CacheDirectory;
		string strokeOrderCacheDirectory = Path.Combine(cacheDirectory, "stroke-order");
		Assert.True(Directory.Exists(strokeOrderCacheDirectory), "Stroke order cache directory should exist");
		
		string[] cacheFiles = Directory.GetFiles(strokeOrderCacheDirectory, "*.gif");
		Assert.Single(cacheFiles); // Should have exactly one cache file
		
		// Verify cached file contains the expected data
		byte[] cachedData = await File.ReadAllBytesAsync(cacheFiles[0]);
		Assert.Equal(expectedBytes, cachedData);
	}

	[Fact]
	public async Task GetStrokeOrderAnimationBytesAsync_ShouldUseDiskCache_OnSubsequentCalls()
	{
		// Arrange
		string character = "く";
		byte[] expectedBytes = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF header

		_httpMessageHandlerMock
			.Protected()
			.Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.OK,
				Content = new ByteArrayContent(expectedBytes)
			});

		// Act - First call should download and cache
		byte[]? result1 = await _strokeOrderService.GetStrokeOrderAnimationBytesAsync(character);

		// Create a new HTTP client and service instance to simulate restart (clears in-memory cache)
		var newHttpMessageHandlerMock = new Mock<HttpMessageHandler>();
		var newHttpClient = new HttpClient(newHttpMessageHandlerMock.Object);
		var newService = new StrokeOrderService(_loggerMock.Object, newHttpClient, _cacheOptionsMock.Object);

		// Act - Second call should use disk cache (no HTTP request to the new handler)
		byte[]? result2 = await newService.GetStrokeOrderAnimationBytesAsync(character);

		// Assert
		Assert.NotNull(result1);
		Assert.NotNull(result2);
		Assert.Equal(expectedBytes, result1);
		Assert.Equal(expectedBytes, result2);

		// Verify original HTTP was called only once (for the first download)
		_httpMessageHandlerMock
			.Protected()
			.Verify(
				"SendAsync",
				Times.Once(),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>());

		// Verify new HTTP client was never called (used disk cache)
		newHttpMessageHandlerMock
			.Protected()
			.Verify(
				"SendAsync",
				Times.Never(),
				ItExpr.IsAny<HttpRequestMessage>(),
				ItExpr.IsAny<CancellationToken>());
	}

	[Theory]
	[InlineData("さ")]
	[InlineData("し")]
	[InlineData("す")]
	[InlineData("せ")]
	[InlineData("そ")]
	public void SupportedCharacters_ShouldIncludeBasicHiragana(string character)
	{
		// Act
		IEnumerable<string> supportedCharacters = _strokeOrderService.GetSupportedCharacters();

		// Assert
		Assert.Contains(character, supportedCharacters);
	}


}