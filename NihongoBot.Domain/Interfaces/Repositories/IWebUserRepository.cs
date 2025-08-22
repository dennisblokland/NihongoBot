using NihongoBot.Domain.Aggregates.WebUser;

namespace NihongoBot.Domain.Interfaces.Repositories;

public interface IWebUserRepository : IDomainRepository<WebUser, Guid>
{
	Task<WebUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
	Task<WebUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
	Task<IEnumerable<WebUser>> GetEnabledUsersAsync(CancellationToken cancellationToken = default);
	Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
	Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}