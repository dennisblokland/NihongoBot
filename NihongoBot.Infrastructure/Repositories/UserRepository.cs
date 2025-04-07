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
}
