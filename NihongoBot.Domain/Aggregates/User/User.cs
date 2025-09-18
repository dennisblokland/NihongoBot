using NihongoBot.Domain.Base;
using System.Text.Json;

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

		// Character selection for practice - JSON string containing list of enabled characters
		// Default is null which means all characters are enabled (backward compatibility)
		public string? EnabledCharacters { get; set; } = null;

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
			// Get list of all valid hiragana characters
			List<string> allHiraganaCharacters = new List<string>
			{
				"a", "i", "u", "e", "o",
				"ka", "ki", "ku", "ke", "ko",
				"sa", "shi", "su", "se", "so",
				"ta", "chi", "tsu", "te", "to",
				"na", "ni", "nu", "ne", "no",
				"ha", "hi", "fu", "he", "ho",
				"ma", "mi", "mu", "me", "mo",
				"ya", "yu", "yo",
				"ra", "ri", "ru", "re", "ro",
				"wa", "wo", "n",
				"kya", "kyu", "kyo",
				"sha", "shu", "sho",
				"cha", "chu", "cho",
				"nya", "nyu", "nyo",
				"hya", "hyu", "hyo",
				"mya", "myu", "myo",
				"rya", "ryu", "ryo"
			};

			string lowerCharacter = character.ToLower();
			if (!allHiraganaCharacters.Contains(lowerCharacter))
			{
				throw new ArgumentException($"Invalid character: {character}");
			}

			List<string> currentEnabledCharacters = GetEnabledCharacters();
			
			if (enabled)
			{
				if (!currentEnabledCharacters.Contains(lowerCharacter))
				{
					currentEnabledCharacters.Add(lowerCharacter);
				}
			}
			else
			{
				currentEnabledCharacters.Remove(lowerCharacter);
			}

			// Store as JSON string, or null if all characters are enabled (for backward compatibility)
			if (currentEnabledCharacters.Count == allHiraganaCharacters.Count)
			{
				EnabledCharacters = null; // All enabled
			}
			else
			{
				EnabledCharacters = JsonSerializer.Serialize(currentEnabledCharacters);
			}
		}

		public List<string> GetEnabledCharacters()
		{
			// If EnabledCharacters is null, return all characters (default behavior)
			if (EnabledCharacters == null)
			{
				return new List<string>
				{
					"a", "i", "u", "e", "o",
					"ka", "ki", "ku", "ke", "ko",
					"sa", "shi", "su", "se", "so",
					"ta", "chi", "tsu", "te", "to",
					"na", "ni", "nu", "ne", "no",
					"ha", "hi", "fu", "he", "ho",
					"ma", "mi", "mu", "me", "mo",
					"ya", "yu", "yo",
					"ra", "ri", "ru", "re", "ro",
					"wa", "wo", "n",
					"kya", "kyu", "kyo",
					"sha", "shu", "sho",
					"cha", "chu", "cho",
					"nya", "nyu", "nyo",
					"hya", "hyu", "hyo",
					"mya", "myu", "myo",
					"rya", "ryu", "ryo"
				};
			}

			try
			{
				return JsonSerializer.Deserialize<List<string>>(EnabledCharacters) ?? new List<string>();
			}
			catch
			{
				// If deserialization fails, return all characters as fallback
				return new List<string>
				{
					"a", "i", "u", "e", "o",
					"ka", "ki", "ku", "ke", "ko",
					"sa", "shi", "su", "se", "so",
					"ta", "chi", "tsu", "te", "to",
					"na", "ni", "nu", "ne", "no",
					"ha", "hi", "fu", "he", "ho",
					"ma", "mi", "mu", "me", "mo",
					"ya", "yu", "yo",
					"ra", "ri", "ru", "re", "ro",
					"wa", "wo", "n",
					"kya", "kyu", "kyo",
					"sha", "shu", "sho",
					"cha", "chu", "cho",
					"nya", "nyu", "nyo",
					"hya", "hyu", "hyo",
					"mya", "myu", "myo",
					"rya", "ryu", "ryo"
				};
			}
		}

		public bool IsCharacterEnabled(string character)
		{
			return GetEnabledCharacters().Contains(character.ToLower());
		}

		public static Dictionary<string, string> GetCharacterDisplayNames()
		{
			return new Dictionary<string, string>
			{
				{ "a", "A (あ)" }, { "i", "I (い)" }, { "u", "U (う)" }, { "e", "E (え)" }, { "o", "O (お)" },
				{ "ka", "Ka (か)" }, { "ki", "Ki (き)" }, { "ku", "Ku (く)" }, { "ke", "Ke (け)" }, { "ko", "Ko (こ)" },
				{ "sa", "Sa (さ)" }, { "shi", "Shi (し)" }, { "su", "Su (す)" }, { "se", "Se (せ)" }, { "so", "So (そ)" },
				{ "ta", "Ta (た)" }, { "chi", "Chi (ち)" }, { "tsu", "Tsu (つ)" }, { "te", "Te (て)" }, { "to", "To (と)" },
				{ "na", "Na (な)" }, { "ni", "Ni (に)" }, { "nu", "Nu (ぬ)" }, { "ne", "Ne (ね)" }, { "no", "No (の)" },
				{ "ha", "Ha (は)" }, { "hi", "Hi (ひ)" }, { "fu", "Fu (ふ)" }, { "he", "He (へ)" }, { "ho", "Ho (ほ)" },
				{ "ma", "Ma (ま)" }, { "mi", "Mi (み)" }, { "mu", "Mu (む)" }, { "me", "Me (め)" }, { "mo", "Mo (も)" },
				{ "ya", "Ya (や)" }, { "yu", "Yu (ゆ)" }, { "yo", "Yo (よ)" },
				{ "ra", "Ra (ら)" }, { "ri", "Ri (り)" }, { "ru", "Ru (る)" }, { "re", "Re (れ)" }, { "ro", "Ro (ろ)" },
				{ "wa", "Wa (わ)" }, { "wo", "Wo (を)" }, { "n", "N (ん)" },
				{ "kya", "Kya (きゃ)" }, { "kyu", "Kyu (きゅ)" }, { "kyo", "Kyo (きょ)" },
				{ "sha", "Sha (しゃ)" }, { "shu", "Shu (しゅ)" }, { "sho", "Sho (しょ)" },
				{ "cha", "Cha (ちゃ)" }, { "chu", "Chu (ちゅ)" }, { "cho", "Cho (ちょ)" },
				{ "nya", "Nya (にゃ)" }, { "nyu", "Nyu (にゅ)" }, { "nyo", "Nyo (にょ)" },
				{ "hya", "Hya (ひゃ)" }, { "hyu", "Hyu (ひゅ)" }, { "hyo", "Hyo (ひょ)" },
				{ "mya", "Mya (みゃ)" }, { "myu", "Myu (みゅ)" }, { "myo", "Myo (みょ)" },
				{ "rya", "Rya (りゃ)" }, { "ryu", "Ryu (りゅ)" }, { "ryo", "Ryo (りょ)" }
			};
		}
	}
}
