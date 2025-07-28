using NihongoBot.Application.Services;
using NihongoBot.Application.Workers;
using NihongoBot.Infrastructure.Extentions;

class Program
{
    public static async Task Main(string[] args)
    {
		IHost host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
			IConfigurationRoot config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			services.AddSingleton<IConfiguration>(config);

			services.AddInfrastructureServices();

            services.AddSingleton<BotService>();
            services.AddSingleton<HiraganaService>();
            services.AddHostedService<TelegramBotWorker>();
            services.AddHostedService<ImageCacheWarmupService>();
        });
}
