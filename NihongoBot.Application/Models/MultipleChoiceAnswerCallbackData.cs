using NihongoBot.Application.Enums;

namespace NihongoBot.Application.Models;

public class MultipleChoiceAnswerCallbackData : AbstractCallbackData 
{
	public Guid QuestionId { get; set; }
	public string SelectedAnswer { get; set; } = string.Empty;
	public override CallBackType Type => CallBackType.MultipleChoiceAnswer;
}