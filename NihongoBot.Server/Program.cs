using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using NihongoBot.Application.Services;
using NihongoBot.Application.Services.Admin;
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

// Add Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 6;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;
	options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login";
	options.LogoutPath = "/Account/Logout";
	options.AccessDeniedPath = "/Account/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromHours(24);
	options.SlidingExpiration = true;
});

// Add admin services
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<TelegramUserService>();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddScoped<SystemMonitoringService>();
builder.Services.AddScoped<DatabaseSeederService>();

builder.Services.AddHangfireServer(options => options.ServerName = "Hangfire Server");

builder.Services.AddScoped<HiraganaService>();

// Move seeding to after app is built to have proper async context
WebApplication app = builder.Build();

// Database migration and seeding
await InitializeApplicationAsync(app);

async Task InitializeApplicationAsync(WebApplication application)
{
	using IServiceScope scope = application.Services.CreateScope();
	AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	
	// Use different database initialization strategies based on provider
	if (dbContext.Database.IsSqlite())
	{
		// For SQLite (development), ensure database is created
		await dbContext.Database.EnsureCreatedAsync();
	}
	else
	{
		// For PostgreSQL (production), use migrations
		dbContext.Database.Migrate();
	}
	
	// Seed default admin user
	DatabaseSeederService seederService = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
	await seederService.SeedDefaultAdminUserAsync();
	
	HangfireSchedulerService hangfireSchedulerService = scope.ServiceProvider.GetRequiredService<HangfireSchedulerService>();
	await hangfireSchedulerService.InitializeSchedulerAsync();
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("/", () => Results.Redirect("/admin"));

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
