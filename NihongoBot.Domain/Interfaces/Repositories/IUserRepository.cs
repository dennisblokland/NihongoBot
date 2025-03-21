namespace NihongoBot.Domain.Interfaces.Repositories;

public interface IUserRepository : IDomainRepository<User, Guid>
{
	Task<User?> GetByTelegramIdAsync(long chatId, CancellationToken cancellationToken = default);
}
