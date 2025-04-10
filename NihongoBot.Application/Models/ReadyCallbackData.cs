using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class ReadyCallbackData : AbstractCallbackData 
{
	public Guid QuestionId { get; set; }
	public override CallBackType Type  => CallBackType.ReadyForQuestion;
}

