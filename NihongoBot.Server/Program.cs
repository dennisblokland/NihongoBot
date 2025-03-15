using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.EntityFrameworkCore;

using NihongoBot.Application.Services;
using NihongoBot.Persistence;

using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot config = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile("appsettings.Development.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

string connectionString = config.GetConnectionString("NihongoBotDB");

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseNpgsql(connectionString));

using (IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope())
{
	AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	dbContext.Database.Migrate();
}


builder.Services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
								.UseSimpleAssemblyNameTypeSerializer()
								.UseDefaultTypeSerializer()
								.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer(options => options.ServerName = "Hangfire Server");




builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(config["TelegramBotToken"]));
builder.Services.AddScoped<HiraganaService>();

builder.Services.AddHostedService<HangfireSchedulerService>();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");


app.UseHangfireDashboard();
app.Run();
