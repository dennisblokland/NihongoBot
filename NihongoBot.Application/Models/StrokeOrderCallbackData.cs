using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class StrokeOrderCallbackData : AbstractCallbackData
{
	public Guid QuestionId { get; set; }
	public string Character { get; set; } = string.Empty;

	public override CallBackType Type => CallBackType.ShowStrokeOrder;
}