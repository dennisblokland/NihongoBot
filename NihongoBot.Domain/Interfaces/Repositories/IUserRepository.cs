namespace NihongoBot.Domain.Interfaces.Repositories;

public interface IUserRepository : IDomainRepository<User, Guid>
{
	Task<User?> GetByTelegramIdAsync(long chatId, CancellationToken cancellationToken = default);
	Task<IEnumerable<User>> GetTop10UsersByHighestStreakAsync(CancellationToken cancellationToken = default);
	Task<int> GetUserStreakRankAsync(Guid id, CancellationToken cancellationToken);
}
