using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NihongoBot.Domain.Aggregates.AdminUser;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Application.Services.Admin
{
	public class DatabaseSeederService
	{
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IAdminUserRepository _adminUserRepository;
		private readonly ILogger<DatabaseSeederService> _logger;
		private readonly IConfiguration _configuration;

		public DatabaseSeederService(
			UserManager<IdentityUser> userManager,
			IAdminUserRepository adminUserRepository,
			ILogger<DatabaseSeederService> logger,
			IConfiguration configuration)
		{
			_userManager = userManager;
			_adminUserRepository = adminUserRepository;
			_logger = logger;
			_configuration = configuration;
		}

		public async Task SeedDefaultAdminUserAsync()
		{
			try
			{
				// Get default admin credentials from configuration
				string defaultEmail = _configuration["DefaultAdmin:Email"] ?? "admin@nihongobot.com";
				string defaultPassword = _configuration["DefaultAdmin:Password"] ?? "Admin123!";
				string defaultUsername = _configuration["DefaultAdmin:Username"] ?? "admin";

				// Check if default admin user already exists in Identity
				IdentityUser? existingIdentityUser = await _userManager.FindByEmailAsync(defaultEmail);
				if (existingIdentityUser != null)
				{
					_logger.LogInformation("Default admin user already exists in Identity system");
					return;
				}

				// Create the Identity user
				IdentityUser identityUser = new IdentityUser
				{
					UserName = defaultEmail,
					Email = defaultEmail,
					EmailConfirmed = true
				};

				IdentityResult result = await _userManager.CreateAsync(identityUser, defaultPassword);
				if (!result.Succeeded)
				{
					_logger.LogError("Failed to create default admin Identity user: {Errors}", 
						string.Join(", ", result.Errors.Select(e => e.Description)));
					return;
				}

				// Create the AdminUser entity
				AdminUser adminUser = new AdminUser(defaultEmail, defaultUsername);

				await _adminUserRepository.AddAsync(adminUser, CancellationToken.None);
				await _adminUserRepository.SaveChangesAsync(CancellationToken.None);

				_logger.LogInformation("Default admin user created successfully with email: {Email}", defaultEmail);
				_logger.LogInformation("Default admin password: {Password}", defaultPassword);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while seeding default admin user");
				throw;
			}
		}
	}
}