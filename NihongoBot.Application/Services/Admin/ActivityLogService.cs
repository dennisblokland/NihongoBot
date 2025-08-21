using NihongoBot.Domain.Aggregates.ActivityLog;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Application.Services.Admin
{
	public class ActivityLogService
	{
		private readonly IActivityLogRepository _activityLogRepository;

		public ActivityLogService(IActivityLogRepository activityLogRepository)
		{
			_activityLogRepository = activityLogRepository;
		}

		public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 100, CancellationToken cancellationToken = default)
		{
			return await _activityLogRepository.GetRecentAsync(count, cancellationToken);
		}

		public async Task<IEnumerable<ActivityLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
		{
			return await _activityLogRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
		}

		public async Task<IEnumerable<ActivityLog>> GetByUserAsync(string userId, string userType, CancellationToken cancellationToken = default)
		{
			return await _activityLogRepository.GetByUserAsync(userId, userType, cancellationToken);
		}

		public async Task<IEnumerable<ActivityLog>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
		{
			return await _activityLogRepository.GetByDateRangeAsync(from, to, cancellationToken);
		}

		public async Task<Dictionary<string, int>> GetActionCountsAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
		{
			return await _activityLogRepository.GetActionCountsAsync(from, to, cancellationToken);
		}

		public async Task LogActivityAsync(string action, string entityType, string entityId, string? details = null, string? userId = null, string? userType = null, CancellationToken cancellationToken = default)
		{
			var activityLog = new ActivityLog(action, entityType, entityId, details);
			
			if (userId != null && userType != null)
			{
				activityLog.SetUserContext(userId, userType);
			}

			await _activityLogRepository.AddAsync(activityLog, cancellationToken);
			await _activityLogRepository.SaveChangesAsync(cancellationToken);
		}

		public async Task<ActivityStatistics> GetStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
		{
			from ??= DateTime.UtcNow.AddDays(-30); // Default to last 30 days
			to ??= DateTime.UtcNow;

			var logs = await _activityLogRepository.GetByDateRangeAsync(from.Value, to.Value, cancellationToken);
			var actionCounts = await _activityLogRepository.GetActionCountsAsync(from.Value, to.Value, cancellationToken);

			var telegramUserActions = logs.Where(l => l.UserType == "Telegram").Count();
			var adminUserActions = logs.Where(l => l.UserType == "Admin").Count();
			var systemActions = logs.Where(l => l.UserType == "System" || string.IsNullOrEmpty(l.UserType)).Count();

			return new ActivityStatistics
			{
				TotalActions = logs.Count(),
				TelegramUserActions = telegramUserActions,
				AdminUserActions = adminUserActions,
				SystemActions = systemActions,
				ActionCounts = actionCounts,
				DateRange = new DateRange { From = from.Value, To = to.Value }
			};
		}
	}

	public class ActivityStatistics
	{
		public int TotalActions { get; set; }
		public int TelegramUserActions { get; set; }
		public int AdminUserActions { get; set; }
		public int SystemActions { get; set; }
		public Dictionary<string, int> ActionCounts { get; set; } = new();
		public DateRange DateRange { get; set; } = new();
	}

	public class DateRange
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }
	}
}