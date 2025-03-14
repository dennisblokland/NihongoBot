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
		public DateTime SentAt { get; set; }
		public bool IsAnswered { get; set; }
		public bool IsExpired { get; set; }

		public int Attempts { get; set; }
	}

}
