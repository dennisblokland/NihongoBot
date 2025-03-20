
namespace NihongoBot.Shared.Options;

public class ApplicationOptions : IConfigOptions
{
	public const string SectionKey = "ApplicationOptions";
	public string TelegramBotToken { get; set; } = null!;
}
