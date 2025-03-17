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

		public void IncreaseStreak()
		{
			Streak++;
		}

		public void ResetStreak()
		{
			Streak = 0;
		}
    }
}
