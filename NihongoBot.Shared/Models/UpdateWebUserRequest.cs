using System.ComponentModel.DataAnnotations;

namespace NihongoBot.Shared.Models;

public class UpdateWebUserRequest
{
	[Required]
	[StringLength(100, MinimumLength = 1)]
	public string FirstName { get; set; } = string.Empty;

	[Required]
	[StringLength(100, MinimumLength = 1)]
	public string LastName { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	public bool IsEnabled { get; set; }
}