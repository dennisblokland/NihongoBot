using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NihongoBot.Application.Services;
using NihongoBot.Domain.Aggregates.WebUser;
using NihongoBot.Shared.Models;

namespace NihongoBot.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WebUsersController : ControllerBase
{
	private readonly WebUserService _webUserService;

	public WebUsersController(WebUserService webUserService)
	{
		_webUserService = webUserService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		IEnumerable<WebUser> webUsers = await _webUserService.GetAllWebUsersAsync();
		List<WebUserDto> webUserDtos = webUsers.Select(MapToDto).ToList();
		return Ok(webUserDtos);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		WebUser? webUser = await _webUserService.GetWebUserByIdAsync(id);
		if (webUser == null)
		{
			return NotFound();
		}

		return Ok(MapToDto(webUser));
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateWebUserRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		// Check if email or username already exists
		if (await _webUserService.EmailExistsAsync(request.Email))
		{
			return BadRequest("Email already exists");
		}

		if (await _webUserService.UsernameExistsAsync(request.Username))
		{
			return BadRequest("Username already exists");
		}

		try
		{
			WebUser webUser = await _webUserService.CreateWebUserAsync(
				request.Email,
				request.Username,
				request.FirstName,
				request.LastName,
				request.Password);

			return Ok(MapToDto(webUser));
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWebUserRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		WebUser? webUser = await _webUserService.GetWebUserByIdAsync(id);
		if (webUser == null)
		{
			return NotFound();
		}

		// Check if email is being changed and already exists
		if (webUser.Email != request.Email && await _webUserService.EmailExistsAsync(request.Email))
		{
			return BadRequest("Email already exists");
		}

		webUser.UpdateProfile(request.FirstName, request.LastName);
		webUser.UpdateEmail(request.Email);

		if (request.IsEnabled)
		{
			webUser.Enable();
		}
		else
		{
			webUser.Disable();
		}

		await _webUserService.UpdateWebUserAsync(webUser);

		return Ok(MapToDto(webUser));
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		WebUser? webUser = await _webUserService.GetWebUserByIdAsync(id);
		if (webUser == null)
		{
			return NotFound();
		}

		await _webUserService.DeleteWebUserAsync(id);
		return NoContent();
	}

	private static WebUserDto MapToDto(WebUser webUser)
	{
		return new WebUserDto
		{
			Id = webUser.Id,
			Email = webUser.Email,
			Username = webUser.Username,
			FirstName = webUser.FirstName,
			LastName = webUser.LastName,
			IsEnabled = webUser.IsEnabled,
			LastLoginAt = webUser.LastLoginAt,
			CreatedAt = webUser.CreatedAt,
			UpdatedAt = webUser.UpdatedAt
		};
	}
}