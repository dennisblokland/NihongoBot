using System.ComponentModel.DataAnnotations;

namespace NihongoBot.Shared.Models;

public class UpdateTelegramUserRequest
{
	[Required]
	[StringLength(64, MinimumLength = 1)]
	public string TimeZone { get; set; } = string.Empty;
}