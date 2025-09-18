using NihongoBot.Domain.Base;

namespace NihongoBot.Domain
{
	public class User : DomainEntity
	{
		public User(long telegramId, string? username)
		{
			TelegramId = telegramId;
			Username = username;
		}
		public long TelegramId { get; private set; }
		public string? Username { get; private set; }

		public int Streak { get; private set; } = 0;

		public int QuestionsPerDay { get; set; } = 2;
		public bool WordOfTheDayEnabled { get; set; } = true;

		// Character selection for practice (Ka, Ki, Ku, Ke, Ko)
		public bool KaEnabled { get; set; } = true;
		public bool KiEnabled { get; set; } = true;
		public bool KuEnabled { get; set; } = true;
		public bool KeEnabled { get; set; } = true;
		public bool KoEnabled { get; set; } = true;

		public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

		public void IncreaseStreak()
		{
			Streak++;
		}

		public void ResetStreak()
		{
			Streak = 0;
		}

		public void UpdateQuestionsPerDay(int questionsPerDay)
		{
			QuestionsPerDay = questionsPerDay;
		}

		public void UpdateWordOfTheDayEnabled(bool enabled)
		{
			WordOfTheDayEnabled = enabled;
		}

		public void UpdateTimeZone(TimeZoneInfo timeZone)
		{
			TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
		}

		public void UpdateCharacterSelection(string character, bool enabled)
		{
			switch (character.ToLower())
			{
				case "ka":
					KaEnabled = enabled;
					break;
				case "ki":
					KiEnabled = enabled;
					break;
				case "ku":
					KuEnabled = enabled;
					break;
				case "ke":
					KeEnabled = enabled;
					break;
				case "ko":
					KoEnabled = enabled;
					break;
				default:
					throw new ArgumentException($"Invalid character: {character}");
			}
		}

		public List<string> GetEnabledCharacters()
		{
			List<string> enabledCharacters = new List<string>();
			if (KaEnabled) enabledCharacters.Add("ka");
			if (KiEnabled) enabledCharacters.Add("ki");
			if (KuEnabled) enabledCharacters.Add("ku");
			if (KeEnabled) enabledCharacters.Add("ke");
			if (KoEnabled) enabledCharacters.Add("ko");
			return enabledCharacters;
		}
	}
}
