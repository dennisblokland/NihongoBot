namespace NihongoBot.Domain.Interfaces.Repositories;

public interface IUserRepository : IDomainRepository<User, Guid>
{
	Task<User?> GetByTelegramIdAsync(long chatId, CancellationToken cancellationToken = default);
	Task UpdateQuestionsPerDayAsync(Guid userId, int questionsPerDay, CancellationToken cancellationToken = default);
	Task UpdateWordOfTheDayEnabledAsync(Guid userId, bool enabled, CancellationToken cancellationToken = default);
}
