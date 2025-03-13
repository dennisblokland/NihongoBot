using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NihongoBot.Application.Services;
using NihongoBot.Persistence;

class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            string connectionString = config.GetConnectionString("NihongoBot");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));


            services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config["TelegramBotToken"]));
            services.AddSingleton<BotService>();
            services.AddSingleton<HiraganaService>();
            services.AddHostedService<TelegramBotWorker>();
        });
}
