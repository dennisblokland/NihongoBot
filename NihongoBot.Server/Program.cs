using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;

using Microsoft.EntityFrameworkCore;

using NihongoBot.Application.Services;
using NihongoBot.Persistence;
using NihongoBot.Infrastructure.Extentions;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables()
			.Build();

builder.Services.AddInfrastructureServices();
builder.Services.AddHangfireServer(options => options.ServerName = "Hangfire Server");

builder.Services.AddScoped<HiraganaService>();

using (IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope())
{
	AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	dbContext.Database.Migrate();
	
	HangfireSchedulerService hangfireSchedulerService = scope.ServiceProvider.GetRequiredService<HangfireSchedulerService>();
	await hangfireSchedulerService.InitializeSchedulerAsync();
}

WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	Authorization =
	[
		new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
		{
			RequireSsl = false,
			SslRedirect = false,
			LoginCaseSensitive = false,
			Users =
			[
				new BasicAuthAuthorizationUser
				{
					Login = "admin",
					PasswordClear = "admin",
				},
			],
		}),
	],
});
app.Run();
