using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NihongoBot.Persistence.Identity;
using NihongoBot.Shared.Models;

namespace NihongoBot.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly UserManager<ApplicationUser> _userManager;

	public AuthController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
	{
		_signInManager = signInManager;
		_userManager = userManager;
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		ApplicationUser? user = await _userManager.FindByNameAsync(request.Username);
		if (user == null)
		{
			return Unauthorized("Invalid username or password");
		}

		Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(
			user, request.Password, request.RememberMe, lockoutOnFailure: true);

		if (result.Succeeded)
		{
			return Ok(new { message = "Login successful" });
		}

		if (result.IsLockedOut)
		{
			return BadRequest("Account is locked out");
		}

		return Unauthorized("Invalid username or password");
	}

	[HttpPost("logout")]
	[Authorize]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();
		return Ok(new { message = "Logout successful" });
	}

	[HttpGet("user")]
	[Authorize]
	public async Task<IActionResult> GetCurrentUser()
	{
		ApplicationUser? user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return Unauthorized();
		}

		return Ok(new
		{
			id = user.Id,
			username = user.UserName,
			email = user.Email
		});
	}
}