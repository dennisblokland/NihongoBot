using Microsoft.AspNetCore.Identity;
using NihongoBot.Domain.Aggregates.WebUser;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Persistence.Identity;

namespace NihongoBot.Application.Services;

public class WebUserService
{
	private readonly IWebUserRepository _webUserRepository;
	private readonly UserManager<ApplicationUser> _userManager;

	public WebUserService(IWebUserRepository webUserRepository, UserManager<ApplicationUser> userManager)
	{
		_webUserRepository = webUserRepository;
		_userManager = userManager;
	}

	public async Task<WebUser> CreateWebUserAsync(string email, string username, string firstName, string lastName, string password, CancellationToken cancellationToken = default)
	{
		// Create domain entity
		WebUser webUser = new WebUser(email, username, firstName, lastName);
		await _webUserRepository.AddAsync(webUser, cancellationToken);
		await _webUserRepository.SaveChangesAsync(cancellationToken);

		// Create identity user
		ApplicationUser applicationUser = new ApplicationUser
		{
			Id = Guid.NewGuid(),
			WebUserId = webUser.Id,
			UserName = username,
			Email = email,
			EmailConfirmed = true
		};

		IdentityResult result = await _userManager.CreateAsync(applicationUser, password);
		if (!result.Succeeded)
		{
			throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
		}

		return webUser;
	}

	public async Task<IEnumerable<WebUser>> GetAllWebUsersAsync(CancellationToken cancellationToken = default)
	{
		return await _webUserRepository.GetAsync(cancellationToken);
	}

	public async Task<WebUser?> GetWebUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _webUserRepository.FindByIdAsync(id, cancellationToken);
	}

	public async Task<WebUser?> GetWebUserByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		return await _webUserRepository.GetByEmailAsync(email, cancellationToken);
	}

	public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
	{
		return await _webUserRepository.EmailExistsAsync(email, cancellationToken);
	}

	public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
	{
		return await _webUserRepository.UsernameExistsAsync(username, cancellationToken);
	}

	public async Task UpdateWebUserAsync(WebUser webUser, CancellationToken cancellationToken = default)
	{
		_webUserRepository.Update(webUser);
		await _webUserRepository.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteWebUserAsync(Guid id, CancellationToken cancellationToken = default)
	{
		WebUser? webUser = await _webUserRepository.FindByIdAsync(id, cancellationToken);
		if (webUser != null)
		{
			// Find and delete the corresponding Identity user
			ApplicationUser? applicationUser = await _userManager.FindByEmailAsync(webUser.Email);
			if (applicationUser != null)
			{
				await _userManager.DeleteAsync(applicationUser);
			}

			_webUserRepository.Remove(webUser);
			await _webUserRepository.SaveChangesAsync(cancellationToken);
		}
	}
}