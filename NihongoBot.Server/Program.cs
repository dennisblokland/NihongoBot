using Hangfire;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using NihongoBot.Application.Services;
using NihongoBot.Persistence;
using NihongoBot.Persistence.Identity;
using NihongoBot.Infrastructure.Extentions;
using NihongoBot.Shared.Options;
using NihongoBot.Server.Security;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
			.AddEnvironmentVariables()
			.Build();

builder.Services.AddInfrastructureServices();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
	// Password settings
	options.Password.RequireDigit = true;
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;

	// Lockout settings
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = true;

	// User settings
	options.User.RequireUniqueEmail = true;
});

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/admin/login";
	options.LogoutPath = "/admin/logout";
	options.AccessDeniedPath = "/admin/accessdenied";
	options.ExpireTimeSpan = TimeSpan.FromHours(8);
	options.SlidingExpiration = true;
});

builder.Services.AddHangfireServer(options => options.ServerName = "Hangfire Server");

builder.Services.AddScoped<HiraganaService>();
builder.Services.AddScoped<WebUserService>();
builder.Services.AddScoped<AdminInitializationService>();

// Add admin settings
builder.Services.Configure<AdminSettings>(
	builder.Configuration.GetSection("AdminSettings"));

using (IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope())
{
	AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	dbContext.Database.Migrate();

	HangfireSchedulerService hangfireSchedulerService = scope.ServiceProvider.GetRequiredService<HangfireSchedulerService>();
	await hangfireSchedulerService.InitializeSchedulerAsync();

	// Initialize default admin user
	AdminInitializationService adminInitializationService = scope.ServiceProvider.GetRequiredService<AdminInitializationService>();
	await adminInitializationService.InitializeDefaultAdminAsync();
}

WebApplication app = builder.Build();

// Configure authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map API controllers
app.MapControllers();

// Serve Blazor WebAssembly client
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Redirect unauthenticated users hitting /hangfire to the SPA login page
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/hangfire"), branch =>
{
	branch.Use(async (context, next) =>
	{
		bool isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
		if (!isAuthenticated)
		{
			string returnUrl = context.Request.Path + context.Request.QueryString;
			string redirectUrl = $"/admin/login?returnUrl={Uri.EscapeDataString(returnUrl)}";
			context.Response.Redirect(redirectUrl);
			return;
		}

		await next();
	});
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	Authorization = [new CookieDashboardAuthorizationFilter()],
});

// SPA fallback to Blazor index.html
app.MapFallbackToFile("index.html");
app.Run();
