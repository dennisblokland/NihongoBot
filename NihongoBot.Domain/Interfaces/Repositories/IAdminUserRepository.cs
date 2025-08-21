using NihongoBot.Domain.Aggregates.AdminUser;

namespace NihongoBot.Domain.Interfaces.Repositories
{
	public interface IAdminUserRepository : IDomainRepository<AdminUser, Guid>
	{
		Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<AdminUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
		Task<IEnumerable<AdminUser>> GetAllActiveAsync(CancellationToken cancellationToken = default);
		Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<bool> ExistsWithUsernameAsync(string username, CancellationToken cancellationToken = default);
	}
}