using Microsoft.EntityFrameworkCore;
using NihongoBot.Domain.Aggregates.ActivityLog;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Infrastructure.Repositories
{
	public class ActivityLogRepository(IServiceProvider serviceProvider) : AbstractDomainRepository<ActivityLog, Guid>(serviceProvider), IActivityLogRepository
	{
		public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet
				.OrderByDescending(x => x.Timestamp)
				.Take(count)
				.ToListAsync(cancellationToken: cancellationToken);
		}

		public async Task<IEnumerable<ActivityLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet
				.Where(x => x.EntityType == entityType && x.EntityId == entityId)
				.OrderByDescending(x => x.Timestamp)
				.ToListAsync(cancellationToken: cancellationToken);
		}

		public async Task<IEnumerable<ActivityLog>> GetByUserAsync(string userId, string userType, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet
				.Where(x => x.UserId == userId && x.UserType == userType)
				.OrderByDescending(x => x.Timestamp)
				.ToListAsync(cancellationToken: cancellationToken);
		}

		public async Task<IEnumerable<ActivityLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet
				.Where(x => x.Timestamp >= from && x.Timestamp <= to)
				.OrderByDescending(x => x.Timestamp)
				.ToListAsync(cancellationToken: cancellationToken);
		}

		public async Task<Dictionary<string, int>> GetActionCountsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
		{
			return await DatabaseSet
				.Where(x => x.Timestamp >= from && x.Timestamp <= to)
				.GroupBy(x => x.Action)
				.Select(g => new { Action = g.Key, Count = g.Count() })
				.ToDictionaryAsync(x => x.Action, x => x.Count, cancellationToken: cancellationToken);
		}
	}
}