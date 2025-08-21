using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using NihongoBot.Persistence;
using NihongoBot.Shared.Options;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SQLite;
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
		
		// For Hangfire, use SQLite connection string in development or PostgreSQL in production
		string hangfireConnectionString = (string.IsNullOrEmpty(connectionString) || 
		                                   connectionString.Contains("localhost") || 
		                                   connectionString.Contains("Host=localhost"))
			? "Data Source=hangfire_dev.db"
			: connectionString;
		
		services.AddHangfire(hangfireConnectionString);

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
		{
			// Use SQLite for development when no valid PostgreSQL connection string is provided
			// This allows the admin interface to work without PostgreSQL setup
			if (string.IsNullOrEmpty(connectionString) || 
			    connectionString.Contains("localhost") || 
			    connectionString.Contains("Host=localhost"))
			{
				// Use SQLite for development
				options.UseSqlite("Data Source=nihongobot_dev.db");
			}
			else
			{
				// Use PostgreSQL for production
				options.UseNpgsql(connectionString);
			}
		});
	}

	private static void AddHangfire(this IServiceCollection services, string connectionString)
	{
		services.AddHangfire(config =>
		{
			config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
										.UseSimpleAssemblyNameTypeSerializer()
										.UseDefaultTypeSerializer();
			
			// Use different storage providers based on connection string format
			if (connectionString.StartsWith("Data Source=") || connectionString.Contains(".db"))
			{
				// This is a SQLite connection string
				config.UseSQLiteStorage(connectionString);
			}
			else
			{
				// Use PostgreSQL for production
				config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString));
			}
			
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

		// Only register Telegram bot client if a valid token is provided
		// This allows the admin interface to work without a Telegram bot token
		if (!string.IsNullOrEmpty(applicationOptions.TelegramBotToken) && 
		    applicationOptions.TelegramBotToken != "YOUR_TELEGRAM_BOT_TOKEN")
		{
			try
			{
				services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(applicationOptions.TelegramBotToken));
			}
			catch
			{
				// If token is invalid, register a null implementation
				// This prevents the entire application from crashing
				services.AddSingleton<ITelegramBotClient>(provider => null!);
			}
		}
		else
		{
			// Register a null implementation for admin-only scenarios
			services.AddSingleton<ITelegramBotClient>(provider => null!);
		}
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

