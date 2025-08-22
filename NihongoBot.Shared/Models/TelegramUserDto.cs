namespace NihongoBot.Shared.Models;

public class TelegramUserDto
{
	public Guid Id { get; set; }
	public long TelegramId { get; set; }
	public string? Username { get; set; }
	public int Streak { get; set; }
	public int QuestionsPerDay { get; set; }
	public bool WordOfTheDayEnabled { get; set; }
	public string TimeZone { get; set; } = "UTC";
	public DateTime? CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}