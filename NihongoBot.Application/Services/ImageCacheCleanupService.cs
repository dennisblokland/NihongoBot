using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NihongoBot.Application.Interfaces;
using NihongoBot.Shared.Options;

namespace NihongoBot.Application.Services;

/// <summary>
/// Background service that periodically cleans up expired cache files
/// </summary>
public class ImageCacheCleanupService : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<ImageCacheCleanupService> _logger;
	private readonly ImageCacheOptions _options;

	public ImageCacheCleanupService(
		IServiceProvider serviceProvider,
		ILogger<ImageCacheCleanupService> logger,
		IOptions<ImageCacheOptions> options)
	{
		_serviceProvider = serviceProvider;
		_logger = logger;
		_options = options.Value;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_options.EnableCleanup)
		{
			_logger.LogInformation("Image cache cleanup is disabled");
			return;
		}

		_logger.LogInformation("Image cache cleanup service started with interval: {IntervalHours} hours", 
			_options.CleanupIntervalHours);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(TimeSpan.FromHours(_options.CleanupIntervalHours), stoppingToken);

				if (stoppingToken.IsCancellationRequested)
					break;

				using (var scope = _serviceProvider.CreateScope())
				{
					var imageCacheService = scope.ServiceProvider.GetRequiredService<IImageCacheService>();
					
					_logger.LogDebug("Starting scheduled cache cleanup");
					imageCacheService.CleanupExpiredFiles();
				}
			}
			catch (OperationCanceledException)
			{
				// Expected when cancellation is requested
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during cache cleanup");
				// Continue running even if cleanup fails
			}
		}

		_logger.LogInformation("Image cache cleanup service stopped");
	}
}