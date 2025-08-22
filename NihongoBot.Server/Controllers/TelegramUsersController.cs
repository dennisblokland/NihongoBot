using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NihongoBot.Domain;
using NihongoBot.Domain.Interfaces.Repositories;
using NihongoBot.Shared.Models;

namespace NihongoBot.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TelegramUsersController : ControllerBase
{
	private readonly IUserRepository _userRepository;

	public TelegramUsersController(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		IEnumerable<User> users = await _userRepository.GetAsync();
		List<TelegramUserDto> userDtos = users.Select(MapToDto).ToList();
		return Ok(userDtos);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		User? user = await _userRepository.FindByIdAsync(id);
		if (user == null)
		{
			return NotFound();
		}

		return Ok(MapToDto(user));
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		User? user = await _userRepository.FindByIdAsync(id);
		if (user == null)
		{
			return NotFound();
		}

		_userRepository.Remove(user);
		await _userRepository.SaveChangesAsync();
		return NoContent();
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTelegramUserRequest request)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		User? user = await _userRepository.FindByIdAsync(id);
		if (user == null)
		{
			return NotFound();
		}

		try
		{
			TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone);
			user.UpdateTimeZone(timeZone);
			await _userRepository.SaveChangesAsync();
			return Ok(MapToDto(user));
		}
		catch (TimeZoneNotFoundException)
		{
			return BadRequest("Invalid timezone identifier");
		}
		catch (InvalidTimeZoneException)
		{
			return BadRequest("Invalid timezone identifier");
		}
	}

	[HttpGet("top-streaks")]
	public async Task<IActionResult> GetTopStreaks()
	{
		IEnumerable<User> users = await _userRepository.GetTop10UsersByHighestStreakAsync();
		List<TelegramUserDto> userDtos = users.Select(MapToDto).ToList();
		return Ok(userDtos);
	}

	private static TelegramUserDto MapToDto(User user)
	{
		return new TelegramUserDto
		{
			Id = user.Id,
			TelegramId = user.TelegramId,
			Username = user.Username,
			Streak = user.Streak,
			QuestionsPerDay = user.QuestionsPerDay,
			WordOfTheDayEnabled = user.WordOfTheDayEnabled,
			TimeZone = user.TimeZone.Id,
			CreatedAt = user.CreatedAt,
			UpdatedAt = user.UpdatedAt
		};
	}
}