using NihongoBot.Domain;
using NihongoBot.Domain.Interfaces.Repositories;

namespace NihongoBot.Application.Services.Admin
{
	public class TelegramUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IActivityLogRepository _activityLogRepository;

		public TelegramUserService(IUserRepository userRepository, IActivityLogRepository activityLogRepository)
		{
			_userRepository = userRepository;
			_activityLogRepository = activityLogRepository;
		}

		public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _userRepository.GetAsync(cancellationToken);
		}

		public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await _userRepository.FindByIdAsync(id, cancellationToken);
		}

		public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
		{
			return await _userRepository.GetByTelegramIdAsync(telegramId, cancellationToken);
		}

		public async Task<IEnumerable<User>> GetTop10UsersByStreakAsync(CancellationToken cancellationToken = default)
		{
			return await _userRepository.GetTop10UsersByHighestStreakAsync(cancellationToken);
		}

		public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
		{
			var user = await _userRepository.FindByIdAsync(id, cancellationToken);
			if (user == null)
			{
				return false;
			}

			_userRepository.Remove(user);
			await _userRepository.SaveChangesAsync(cancellationToken);

			// Log the activity
			await LogActivityAsync("Delete", "TelegramUser", id.ToString(), $"Deleted Telegram user: {user.Username} (ID: {user.TelegramId})", cancellationToken);

			return true;
		}

		public async Task<UserStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
		{
			var users = await _userRepository.GetAsync(cancellationToken);
			var topUsers = await _userRepository.GetTop10UsersByHighestStreakAsync(cancellationToken);

			return new UserStatistics
			{
				TotalUsers = users.Count(),
				ActiveUsers = users.Count(u => u.Streak > 0),
				AverageStreak = users.Any() ? (decimal)users.Average(u => u.Streak) : 0,
				HighestStreak = users.Any() ? users.Max(u => u.Streak) : 0,
				UsersWithCustomSettings = users.Count(u => u.QuestionsPerDay != 2 || !u.WordOfTheDayEnabled),
				TopUsers = topUsers.Select(u => new UserInfo
				{
					Id = u.Id,
					TelegramId = u.TelegramId,
					Username = u.Username,
					Streak = u.Streak,
					QuestionsPerDay = u.QuestionsPerDay,
					WordOfTheDayEnabled = u.WordOfTheDayEnabled,
					CreatedAt = u.CreatedAt,
					UpdatedAt = u.UpdatedAt
				}).ToList()
			};
		}

		public async Task<IEnumerable<UserInfo>> SearchUsersAsync(string? searchTerm = null, int skip = 0, int take = 50, CancellationToken cancellationToken = default)
		{
			var users = await _userRepository.GetAsync(cancellationToken);
			
			var query = users.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				query = query.Where(u => 
					(u.Username != null && u.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
					u.TelegramId.ToString().Contains(searchTerm));
			}

			return query
				.OrderByDescending(u => u.Streak)
				.ThenByDescending(u => u.UpdatedAt)
				.Skip(skip)
				.Take(take)
				.Select(u => new UserInfo
				{
					Id = u.Id,
					TelegramId = u.TelegramId,
					Username = u.Username,
					Streak = u.Streak,
					QuestionsPerDay = u.QuestionsPerDay,
					WordOfTheDayEnabled = u.WordOfTheDayEnabled,
					CreatedAt = u.CreatedAt,
					UpdatedAt = u.UpdatedAt
				}).ToList();
		}

		private async Task LogActivityAsync(string action, string entityType, string entityId, string? details = null, CancellationToken cancellationToken = default)
		{
			var activityLog = new NihongoBot.Domain.Aggregates.ActivityLog.ActivityLog(action, entityType, entityId, details);
			activityLog.SetUserContext("System", "Admin");
			await _activityLogRepository.AddAsync(activityLog, cancellationToken);
			await _activityLogRepository.SaveChangesAsync(cancellationToken);
		}
	}

	public class UserStatistics
	{
		public int TotalUsers { get; set; }
		public int ActiveUsers { get; set; }
		public decimal AverageStreak { get; set; }
		public int HighestStreak { get; set; }
		public int UsersWithCustomSettings { get; set; }
		public List<UserInfo> TopUsers { get; set; } = new();
	}

	public class UserInfo
	{
		public Guid Id { get; set; }
		public long TelegramId { get; set; }
		public string? Username { get; set; }
		public int Streak { get; set; }
		public int QuestionsPerDay { get; set; }
		public bool WordOfTheDayEnabled { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}