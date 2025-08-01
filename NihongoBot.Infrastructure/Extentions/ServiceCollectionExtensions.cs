using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using NihongoBot.Persistence;
using NihongoBot.Shared.Options;
using Hangfire;
using Hangfire.PostgreSql;
using Telegram.Bot;
using System.Reflection;
using NihongoBot.Infrastructure.Interfaces;
using NihongoBot.Domain.Interfaces;
using NihongoBot.Application.Services;
using NihongoBot.Application.Handlers;
using NihongoBot.Application.Interfaces;

namespace NihongoBot.Infrastructure.Extentions;

public static class ServiceCollectionExtensions
{
	private static readonly Assembly _infrastructureAssembly = typeof(IInfrastructureHook).Assembly;
	private static readonly Assembly _applicationAssembly = typeof(IApplicationHook).Assembly;

	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
	{
		services.AddOptions();

		ConnectionStrings connectionStrings = services.BuildServiceProvider()
		.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue;

		string connectionString = connectionStrings.NihongoBotDB;

		services.AddDatabaseContext(connectionString);
		services.AddHangfire(connectionString);

		services.AddDependencies();
		services.AddTelegramBot();

		return services;
	}

	private static void AddOptions(this IServiceCollection services)
	{
		services.AddOptions<ConnectionStrings>()
			.BindConfiguration(ConnectionStrings.SectionKey);

		services.AddOptions<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.SectionKey);

		services.AddOptions<ImageCacheOptions>()
			.BindConfiguration(ImageCacheOptions.SectionKey);
	}

	private static void AddDatabaseContext(this IServiceCollection services, string connectionString)
	{
		services.AddDbContext<AppDbContext>(options =>
		options.UseNpgsql(connectionString));
	}

	private static void AddHangfire(this IServiceCollection services, string connectionString)
	{
		services.AddHangfire(config =>
		{
			config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
										.UseSimpleAssemblyNameTypeSerializer()
										.UseDefaultTypeSerializer()
										.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
			{
				Attempts = 3,
				OnAttemptsExceeded = AttemptsExceededAction.Delete
			});
		});
		services.AddScoped<RecurringJobManager>();
		services.AddScoped<HangfireSchedulerService>();


	}

	private static void AddTelegramBot(this IServiceCollection services)
	{
		ApplicationOptions applicationOptions = services.BuildServiceProvider()
			.GetRequiredService<IOptionsMonitor<ApplicationOptions>>().CurrentValue;

		services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(applicationOptions.TelegramBotToken));
	}
	private static void AddDependencies(this IServiceCollection services)
	{
		// Aggregate repositories
		services.Scan(scan => scan
			.FromAssemblies(_infrastructureAssembly)
			.AddClasses(classes => classes.AssignableTo<IRepository>())
			.AsImplementedInterfaces()
			.WithScopedLifetime());


		services.Scan(scan => scan
			.FromAssemblies(_applicationAssembly)
			.AddClasses(classes => classes.AssignableTo<ITelegramCommandHandler>())
			.AsSelf()
			.AsImplementedInterfaces()
			.WithScopedLifetime());

		services.Scan(scan => scan
			.FromAssemblies(_applicationAssembly)
			.AddClasses(classes => classes.AssignableTo(typeof(ITelegramCallbackHandler<>)))
			.AsSelf()
			.AsImplementedInterfaces()
			.WithScopedLifetime());

		services.AddScoped<CommandDispatcher>();
		services.AddScoped<CallbackDispatcher>();

		// Image caching services
		services.AddScoped<IImageCacheService, DatabaseImageCacheService>();


		services.AddHttpClient<IJlptVocabApiService, JlptVocabApiService>(client =>
		{
			client.BaseAddress = new Uri("https://jlpt-vocab-api.vercel.app/api/words/");
		});

		services.AddHttpClient<IStrokeOrderService, StrokeOrderService>(client =>
		{
			// Set User-Agent header per Wikipedia's User-Agent policy
			// https://foundation.wikimedia.org/wiki/Policy:Wikimedia_Foundation_User-Agent_Policy
			client.DefaultRequestHeaders.UserAgent.ParseAdd("NihongoBot/1.0 (https://github.com/dennisblokland/NihongoBot; Telegram bot for learning Japanese)");
		});

	}
}

