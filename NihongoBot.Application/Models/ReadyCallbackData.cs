using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class ReadyCallbackData : ICallbackData 
{
	public Guid QuestionId { get; set; }
	public CallBackType Type { get; } = CallBackType.ReadyForQuestion;
}
