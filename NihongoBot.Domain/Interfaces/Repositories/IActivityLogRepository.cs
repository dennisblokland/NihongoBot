using NihongoBot.Domain.Aggregates.ActivityLog;

namespace NihongoBot.Domain.Interfaces.Repositories
{
	public interface IActivityLogRepository : IDomainRepository<ActivityLog, Guid>
	{
		Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default);
		Task<IEnumerable<ActivityLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
		Task<IEnumerable<ActivityLog>> GetByUserAsync(string userId, string userType, CancellationToken cancellationToken = default);
		Task<IEnumerable<ActivityLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
		Task<Dictionary<string, int>> GetActionCountsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
	}
}