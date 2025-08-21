using System.ComponentModel.DataAnnotations;

namespace NihongoBot.Shared.Models;

public class CreateWebUserRequest
{
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	[StringLength(100, MinimumLength = 3)]
	public string Username { get; set; } = string.Empty;

	[Required]
	[StringLength(100, MinimumLength = 1)]
	public string FirstName { get; set; } = string.Empty;

	[Required]
	[StringLength(100, MinimumLength = 1)]
	public string LastName { get; set; } = string.Empty;

	[Required]
	[StringLength(100, MinimumLength = 8)]
	public string Password { get; set; } = string.Empty;

	[Required]
	[Compare("Password")]
	public string ConfirmPassword { get; set; } = string.Empty;
}