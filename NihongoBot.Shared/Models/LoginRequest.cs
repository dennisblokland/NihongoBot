using System.ComponentModel.DataAnnotations;

namespace NihongoBot.Shared.Models;

public class LoginRequest
{
	[Required]
	public string Username { get; set; } = string.Empty;

	[Required]
	public string Password { get; set; } = string.Empty;

	public bool RememberMe { get; set; }
}