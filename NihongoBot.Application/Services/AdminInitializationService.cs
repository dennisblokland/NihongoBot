using Microsoft.Extensions.Options;
using NihongoBot.Application.Services;
using NihongoBot.Shared.Options;

namespace NihongoBot.Application.Services;

public class AdminInitializationService
{
	private readonly WebUserService _webUserService;
	private readonly AdminSettings _adminSettings;

	public AdminInitializationService(WebUserService webUserService, IOptions<AdminSettings> adminSettings)
	{
		_webUserService = webUserService;
		_adminSettings = adminSettings.Value;
	}

	public async Task InitializeDefaultAdminAsync(CancellationToken cancellationToken = default)
	{
		// Check if default admin already exists
		bool emailExists = await _webUserService.EmailExistsAsync(_adminSettings.DefaultAdminEmail, cancellationToken);
		bool usernameExists = await _webUserService.UsernameExistsAsync(_adminSettings.DefaultAdminUsername, cancellationToken);

		if (emailExists || usernameExists)
		{
			return; // Default admin already exists
		}

		// Create default admin user
		await _webUserService.CreateWebUserAsync(
			_adminSettings.DefaultAdminEmail,
			_adminSettings.DefaultAdminUsername,
			_adminSettings.DefaultAdminFirstName,
			_adminSettings.DefaultAdminLastName,
			_adminSettings.DefaultAdminPassword,
			cancellationToken);
	}
}