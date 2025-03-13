using NihongoBot.Domain.Base;

namespace NihongoBot.Domain.Aggregates.User
{
	public class User(long telegramId, string? username) : DomainEntity
	{
		public long TelegramId { get; private set; } = telegramId;
		public string? Username { get; private set; } = username;
		public int Streak { get; private set; } = 0;
	}
}
