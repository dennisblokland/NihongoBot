
using Microsoft.EntityFrameworkCore;

using Name.Infrastructure.Repositories;

using NihongoBot.Domain;

using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class UserRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<User, Guid>(serviceProvider), IUserRepository
{
	public async Task<User?> GetByTelegramIdAsync(long chatId, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.TelegramId == chatId, cancellationToken: cancellationToken);
	}

	public async Task<IEnumerable<User>> GetTop10UsersByHighestStreakAsync(CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
			.OrderByDescending(x => x.Streak)
			.Take(10)
			.ToListAsync(cancellationToken: cancellationToken);
	}

	public async Task<int> GetUserStreakRankAsync(Guid id, CancellationToken cancellationToken)
	{
		var users = DatabaseSet
		.OrderByDescending(u => u.Streak)
		.Select((u, index) => new { u.Id, Rank = index + 1 });

		var result = await users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
		return result?.Rank ?? -1;
	}
}
