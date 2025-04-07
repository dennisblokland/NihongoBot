using Microsoft.EntityFrameworkCore;

using Name.Infrastructure.Repositories;

using NihongoBot.Domain;

using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class UserRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<User, Guid>(serviceProvider), IUserRepository
{
	public async Task<User?> GetByTelegramIdAsync(long chatId, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.TelegramId == chatId);
	}

	public async Task UpdateQuestionsPerDayAsync(Guid userId, int questionsPerDay, CancellationToken cancellationToken = default)
	{
		User? user = await DatabaseSet.FindAsync(new object[] { userId }, cancellationToken);
		if (user != null)
		{
			user.UpdateQuestionsPerDay(questionsPerDay);
			await SaveChangesAsync(cancellationToken);
		}
	}

	public async Task UpdateWordOfTheDayEnabledAsync(Guid userId, bool enabled, CancellationToken cancellationToken = default)
	{
		User? user = await DatabaseSet.FindAsync(new object[] { userId }, cancellationToken);
		if (user != null)
		{
			user.UpdateWordOfTheDayEnabled(enabled);
			await SaveChangesAsync(cancellationToken);
		}
	}
}
