using Microsoft.EntityFrameworkCore;
using NihongoBot.Domain.Aggregates.WebUser;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories;

public class WebUserRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<WebUser, Guid>(serviceProvider), IWebUserRepository
{
	public async Task<WebUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken: cancellationToken);
	}

	public async Task<WebUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.FirstOrDefaultAsync(x => x.Username == username, cancellationToken: cancellationToken);
	}

	public async Task<IEnumerable<WebUser>> GetEnabledUsersAsync(CancellationToken cancellationToken = default)
	{
		return await DatabaseSet
			.Where(x => x.IsEnabled)
			.OrderBy(x => x.Username)
			.ToListAsync(cancellationToken: cancellationToken);
	}

	public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.AnyAsync(x => x.Email == email, cancellationToken: cancellationToken);
	}

	public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
	{
		return await DatabaseSet.AnyAsync(x => x.Username == username, cancellationToken: cancellationToken);
	}
}