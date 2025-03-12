using NihongoBot.Domain.Base;

namespace NihongoBot.Domain
{
    public class User : DomainEntity
    {
        public long TelegramId { get; set; }
        public string Username { get; set; }
    }
}