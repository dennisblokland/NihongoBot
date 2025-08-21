using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Application.Services.Admin
{
	public class SystemMonitoringService
	{
		private readonly IUserRepository _userRepository;
		private readonly IActivityLogRepository _activityLogRepository;

		public SystemMonitoringService(IUserRepository userRepository, IActivityLogRepository activityLogRepository)
		{
			_userRepository = userRepository;
			_activityLogRepository = activityLogRepository;
		}

		public async Task<SystemHealth> GetSystemHealthAsync(CancellationToken cancellationToken = default)
		{
			var uptime = GetUptime();
			var telegramUsers = await _userRepository.GetAsync(cancellationToken);
			var recentErrors = await GetRecentErrorsAsync(cancellationToken);
			var recentActivity = await _activityLogRepository.GetRecentAsync(10, cancellationToken);

			return new SystemHealth
			{
				Uptime = uptime,
				Status = recentErrors.Any() ? "Warning" : "Healthy",
				TotalTelegramUsers = telegramUsers.Count(),
				ActiveTelegramUsers = telegramUsers.Count(u => u.Streak > 0),
				RecentErrors = recentErrors,
				LastActivity = recentActivity.FirstOrDefault()?.Timestamp,
				RecentActivityCount = recentActivity.Count()
			};
		}

		public async Task<TelegramUserStatistics> GetTelegramUserStatisticsAsync(CancellationToken cancellationToken = default)
		{
			var users = await _userRepository.GetAsync(cancellationToken);
			var topUsers = await _userRepository.GetTop10UsersByHighestStreakAsync(cancellationToken);

			return new TelegramUserStatistics
			{
				TotalUsers = users.Count(),
				ActiveUsers = users.Count(u => u.Streak > 0),
				AverageStreak = users.Any() ? (int)users.Average(u => u.Streak) : 0,
				HighestStreak = users.Any() ? users.Max(u => u.Streak) : 0,
				TopUsers = topUsers.Take(5).Select(u => new TopUser
				{
					Username = u.Username ?? "Unknown",
					Streak = u.Streak,
					QuestionsPerDay = u.QuestionsPerDay
				}).ToList()
			};
		}

		public async Task<List<ErrorInfo>> GetRecentErrorsAsync(CancellationToken cancellationToken = default)
		{
			// Get activity logs that might represent errors
			var recentLogs = await _activityLogRepository.GetRecentAsync(100, cancellationToken);
			var errorLogs = recentLogs.Where(l => 
				l.Action.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
				l.Action.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
				l.Action.Contains("Failed", StringComparison.OrdinalIgnoreCase));

			return errorLogs.Select(log => new ErrorInfo
			{
				Timestamp = log.Timestamp,
				Message = log.Details ?? "No details available",
				Type = log.Action,
				EntityType = log.EntityType,
				EntityId = log.EntityId
			}).ToList();
		}

		private TimeSpan GetUptime()
		{
			// Simple uptime calculation based on process start time
			// In a real application, you might want to track this more persistently
			var processStartTime = DateTime.UtcNow.AddHours(-1); // Placeholder
			return DateTime.UtcNow - processStartTime;
		}

		public async Task LogErrorAsync(string errorMessage, string? entityType = null, string? entityId = null, CancellationToken cancellationToken = default)
		{
			var activityLog = new NihongoBot.Domain.Aggregates.ActivityLog.ActivityLog(
				"Error", 
				entityType ?? "System", 
				entityId ?? "Unknown", 
				errorMessage);
			
			activityLog.SetUserContext("System", "System");
			
			await _activityLogRepository.AddAsync(activityLog, cancellationToken);
			await _activityLogRepository.SaveChangesAsync(cancellationToken);
		}
	}

	public class SystemHealth
	{
		public TimeSpan Uptime { get; set; }
		public string Status { get; set; } = string.Empty;
		public int TotalTelegramUsers { get; set; }
		public int ActiveTelegramUsers { get; set; }
		public List<ErrorInfo> RecentErrors { get; set; } = new();
		public DateTime? LastActivity { get; set; }
		public int RecentActivityCount { get; set; }
	}

	public class TelegramUserStatistics
	{
		public int TotalUsers { get; set; }
		public int ActiveUsers { get; set; }
		public int AverageStreak { get; set; }
		public int HighestStreak { get; set; }
		public List<TopUser> TopUsers { get; set; } = new();
	}

	public class TopUser
	{
		public string Username { get; set; } = string.Empty;
		public int Streak { get; set; }
		public int QuestionsPerDay { get; set; }
	}

	public class ErrorInfo
	{
		public DateTime Timestamp { get; set; }
		public string Message { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string EntityType { get; set; } = string.Empty;
		public string EntityId { get; set; } = string.Empty;
	}
}