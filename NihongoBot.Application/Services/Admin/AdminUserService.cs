using Microsoft.AspNetCore.Identity;
using NihongoBot.Domain.Aggregates.AdminUser;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Application.Services.Admin
{
	public class AdminUserService
	{
		private readonly IAdminUserRepository _adminUserRepository;
		private readonly IActivityLogRepository _activityLogRepository;
		private readonly UserManager<IdentityUser> _userManager;

		public AdminUserService(
			IAdminUserRepository adminUserRepository, 
			IActivityLogRepository activityLogRepository,
			UserManager<IdentityUser> userManager)
		{
			_adminUserRepository = adminUserRepository;
			_activityLogRepository = activityLogRepository;
			_userManager = userManager;
		}

		public async Task<IEnumerable<AdminUser>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _adminUserRepository.GetAsync(cancellationToken);
		}

		public async Task<IEnumerable<AdminUser>> GetAllActiveAsync(CancellationToken cancellationToken = default)
		{
			return await _adminUserRepository.GetAllActiveAsync(cancellationToken);
		}

		public async Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await _adminUserRepository.FindByIdAsync(id, cancellationToken);
		}

		public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return await _adminUserRepository.GetByEmailAsync(email, cancellationToken);
		}

		public async Task<(bool Success, string[] Errors)> CreateAsync(string email, string username, string password, CancellationToken cancellationToken = default)
		{
			// Check if email or username already exists
			if (await _adminUserRepository.ExistsWithEmailAsync(email, cancellationToken))
			{
				return (false, new[] { "Email already exists" });
			}

			if (await _adminUserRepository.ExistsWithUsernameAsync(username, cancellationToken))
			{
				return (false, new[] { "Username already exists" });
			}

			// Create Identity user first
			var identityUser = new IdentityUser
			{
				UserName = username,
				Email = email
			};

			var identityResult = await _userManager.CreateAsync(identityUser, password);
			if (!identityResult.Succeeded)
			{
				return (false, identityResult.Errors.Select(e => e.Description).ToArray());
			}

			// Create our AdminUser entity
			var adminUser = new AdminUser(email, username);
			await _adminUserRepository.AddAsync(adminUser, cancellationToken);
			await _adminUserRepository.SaveChangesAsync(cancellationToken);

			// Log the activity
			await LogActivityAsync("Create", "AdminUser", adminUser.Id.ToString(), $"Created admin user: {username}", cancellationToken);

			return (true, Array.Empty<string>());
		}

		public async Task<bool> UpdateAsync(Guid id, string email, string username, CancellationToken cancellationToken = default)
		{
			var adminUser = await _adminUserRepository.FindByIdAsync(id, cancellationToken);
			if (adminUser == null)
			{
				return false;
			}

			// Check for conflicts with other users
			var existingEmailUser = await _adminUserRepository.GetByEmailAsync(email, cancellationToken);
			if (existingEmailUser != null && existingEmailUser.Id != id)
			{
				return false;
			}

			var existingUsernameUser = await _adminUserRepository.GetByUsernameAsync(username, cancellationToken);
			if (existingUsernameUser != null && existingUsernameUser.Id != id)
			{
				return false;
			}

			adminUser.UpdateEmail(email);
			adminUser.UpdateUsername(username);

			_adminUserRepository.Update(adminUser);
			await _adminUserRepository.SaveChangesAsync(cancellationToken);

			// Log the activity
			await LogActivityAsync("Update", "AdminUser", id.ToString(), $"Updated admin user: {username}", cancellationToken);

			return true;
		}

		public async Task<bool> EnableAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var adminUser = await _adminUserRepository.FindByIdAsync(id, cancellationToken);
			if (adminUser == null)
			{
				return false;
			}

			adminUser.Enable();
			_adminUserRepository.Update(adminUser);
			await _adminUserRepository.SaveChangesAsync(cancellationToken);

			// Log the activity
			await LogActivityAsync("Enable", "AdminUser", id.ToString(), $"Enabled admin user: {adminUser.Username}", cancellationToken);

			return true;
		}

		public async Task<bool> DisableAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var adminUser = await _adminUserRepository.FindByIdAsync(id, cancellationToken);
			if (adminUser == null)
			{
				return false;
			}

			adminUser.Disable();
			_adminUserRepository.Update(adminUser);
			await _adminUserRepository.SaveChangesAsync(cancellationToken);

			// Log the activity
			await LogActivityAsync("Disable", "AdminUser", id.ToString(), $"Disabled admin user: {adminUser.Username}", cancellationToken);

			return true;
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var adminUser = await _adminUserRepository.FindByIdAsync(id, cancellationToken);
			if (adminUser == null)
			{
				return false;
			}

			// Also delete the Identity user
			var identityUser = await _userManager.FindByEmailAsync(adminUser.Email);
			if (identityUser != null)
			{
				await _userManager.DeleteAsync(identityUser);
			}

			_adminUserRepository.Remove(adminUser);
			await _adminUserRepository.SaveChangesAsync(cancellationToken);

			// Log the activity
			await LogActivityAsync("Delete", "AdminUser", id.ToString(), $"Deleted admin user: {adminUser.Username}", cancellationToken);

			return true;
		}

		public async Task UpdateLastLoginAsync(string email, CancellationToken cancellationToken = default)
		{
			var adminUser = await _adminUserRepository.GetByEmailAsync(email, cancellationToken);
			if (adminUser != null)
			{
				adminUser.UpdateLastLogin();
				_adminUserRepository.Update(adminUser);
				await _adminUserRepository.SaveChangesAsync(cancellationToken);
			}
		}

		private async Task LogActivityAsync(string action, string entityType, string entityId, string? details = null, CancellationToken cancellationToken = default)
		{
			var activityLog = new NihongoBot.Domain.Aggregates.ActivityLog.ActivityLog(action, entityType, entityId, details);
			activityLog.SetUserContext("System", "Admin");
			await _activityLogRepository.AddAsync(activityLog, cancellationToken);
			await _activityLogRepository.SaveChangesAsync(cancellationToken);
		}
	}
}