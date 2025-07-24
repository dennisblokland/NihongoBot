using NihongoBot.Domain.Base;
using NihongoBot.Domain.Enums;

namespace NihongoBot.Domain.Entities
{
	public class Question : DomainEntity
	{
		public Guid UserId { get; set; }
		public virtual User? User { get; set; }
		public required string QuestionText { get; set; }  // The Hiragana/Katakana/Kanji/Word
		public QuestionType QuestionType { get; set; }
		public required string CorrectAnswer { get; set; }
		public string? MultipleChoiceOptions { get; set; } // JSON array of options for multiple choice questions
		public DateTime SentAt { get; set; }
		public bool IsAnswered { get; set; }
		public bool IsExpired { get; set; }
		public bool IsAccepted { get; set; }
		public int Attempts { get; set; }
		public int TimeLimit { get; set; } // Time limit in minutes for answering the question
		public int MessageId { get; set; }
	}
}
