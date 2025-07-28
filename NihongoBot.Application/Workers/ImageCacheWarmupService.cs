using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NihongoBot.Application.Interfaces;
using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Application.Workers;

/// <summary>
/// Background service that warms up the image cache at application startup
/// </summary>
public class ImageCacheWarmupService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<ImageCacheWarmupService> _logger;

	public ImageCacheWarmupService(
		IServiceProvider serviceProvider,
		ILogger<ImageCacheWarmupService> logger)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			_logger.LogInformation("Starting image cache warmup...");

			using IServiceScope scope = _serviceProvider.CreateScope();
			IKanaRepository kanaRepository = scope.ServiceProvider.GetRequiredService<IKanaRepository>();
			IImageCacheService imageCacheService = scope.ServiceProvider.GetRequiredService<IImageCacheService>();

			// Get all characters from the database
			IEnumerable<Kana> allKanas = await kanaRepository.GetAsync(stoppingToken);
			List<string> characters = allKanas.Select(k => k.Character).ToList();

			// Also get all variant characters
			List<string> variantCharacters = allKanas
				.SelectMany(k => k.Variants)
				.Select(v => v.Character)
				.ToList();

			// Combine all characters
			List<string> allCharacters = characters.Concat(variantCharacters).Distinct().ToList();

			_logger.LogInformation("Warming cache for {Count} unique characters", allCharacters.Count);

			// Warm up the cache
			await imageCacheService.WarmCacheAsync(allCharacters, stoppingToken);

			var stats = imageCacheService.GetCacheStats();
			_logger.LogInformation("Image cache warmup completed. Cache contains {Count} images", stats.TotalEntries);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during image cache warmup");
		}
	}
}