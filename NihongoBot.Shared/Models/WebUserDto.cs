namespace NihongoBot.Shared.Models;

public class WebUserDto
{
	public Guid Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string Username { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public bool IsEnabled { get; set; }
	public DateTime? LastLoginAt { get; set; }
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}