using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NihongoBot.Application.Services;
using NihongoBot.Shared.Options;
using Xunit;

namespace NihongoBot.Application.Tests.Integration;

public class StrokeOrderServiceIntegrationTests
{
	[Fact]
	public void HttpClient_ShouldHaveProperUserAgentHeader_WhenConfiguredThroughDI()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddConsole());
		
		// Add ImageCacheOptions configuration
		services.Configure<ImageCacheOptions>(options =>
		{
			options.CacheDirectory = Path.Combine(Path.GetTempPath(), "test-integration-cache");
			options.CacheExpirationHours = 168;
			options.EnableCleanup = true;
			options.CleanupIntervalHours = 24;
		});
		
		// Add the HttpClient configuration for StrokeOrderService as done in production
		services.AddHttpClient<StrokeOrderService>(client =>
		{
			// Set User-Agent header per Wikipedia's User-Agent policy
			// https://foundation.wikimedia.org/wiki/Policy:Wikimedia_Foundation_User-Agent_Policy
			client.DefaultRequestHeaders.UserAgent.ParseAdd("NihongoBot/1.0 (https://github.com/dennisblokland/NihongoBot; Telegram bot for learning Japanese)");
		});

		var serviceProvider = services.BuildServiceProvider();

		// Act
		var strokeOrderService = serviceProvider.GetRequiredService<StrokeOrderService>();
		
		// Use reflection to access the private _httpClient field to verify its configuration
		var httpClientField = typeof(StrokeOrderService).GetField("_httpClient", 
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		
		Assert.NotNull(httpClientField);
		
		var httpClient = (HttpClient)httpClientField.GetValue(strokeOrderService)!;

		// Assert
		Assert.NotNull(httpClient);
		Assert.True(httpClient.DefaultRequestHeaders.UserAgent.Count > 0, "User-Agent header should be configured");
		
		string userAgentString = httpClient.DefaultRequestHeaders.UserAgent.ToString();
		Assert.Contains("NihongoBot/1.0", userAgentString);
		Assert.Contains("github.com/dennisblokland/NihongoBot", userAgentString);
		Assert.Contains("Telegram bot for learning Japanese", userAgentString);
	}

	[Fact]
	public void StrokeOrderService_ShouldBeRegisteredCorrectly_InDI()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddLogging(builder => builder.AddConsole());
		
		// Add ImageCacheOptions configuration
		services.Configure<ImageCacheOptions>(options =>
		{
			options.CacheDirectory = Path.Combine(Path.GetTempPath(), "test-integration-cache-2");
			options.CacheExpirationHours = 168;
			options.EnableCleanup = true;
			options.CleanupIntervalHours = 24;
		});
		
		// Add the HttpClient configuration as done in ServiceCollectionExtensions
		services.AddHttpClient<StrokeOrderService>(client =>
		{
			client.DefaultRequestHeaders.UserAgent.ParseAdd("NihongoBot/1.0 (https://github.com/dennisblokland/NihongoBot; Telegram bot for learning Japanese)");
		});

		var serviceProvider = services.BuildServiceProvider();

		// Act & Assert
		var strokeOrderService = serviceProvider.GetRequiredService<StrokeOrderService>();
		Assert.NotNull(strokeOrderService);
		Assert.IsType<StrokeOrderService>(strokeOrderService);
	}
}