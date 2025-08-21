using Microsoft.EntityFrameworkCore;
using NihongoBot.Domain.Aggregates.AdminUser;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories
{
	public class AdminUserRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<AdminUser, Guid>(serviceProvider), IAdminUserRepository
	{
		public async Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken: cancellationToken);
		}

		public async Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet.FirstOrDefaultAsync(x => x.Username == username, cancellationToken: cancellationToken);
		}

		public async Task<IEnumerable<AdminUser>> GetAllActiveAsync(CancellationToken cancellationToken = default)
		{
			return await DatabaseSet
				.Where(x => x.IsEnabled)
				.ToListAsync(cancellationToken: cancellationToken);
		}

		public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet.AnyAsync(x => x.Email == email, cancellationToken: cancellationToken);
		}

		public async Task<bool> ExistsWithUsernameAsync(string username, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet.AnyAsync(x => x.Username == username, cancellationToken: cancellationToken);
		}
	}
}