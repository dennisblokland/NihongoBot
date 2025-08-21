using NihongoBot.Domain.Base;

namespace NihongoBot.Domain.Aggregates.ActivityLog
{
	public class ActivityLog : DomainEntity
	{
		public ActivityLog(string action, string entityType, string entityId, string? details = null)
		{
			Action = action;
			EntityType = entityType;
			EntityId = entityId;
			Details = details;
			Timestamp = DateTime.UtcNow;
		}

		public string Action { get; private set; }
		public string EntityType { get; private set; }
		public string EntityId { get; private set; }
		public string? Details { get; private set; }
		public DateTime Timestamp { get; private set; }

		// Optional user context - could be Telegram user ID or Admin user ID
		public string? UserId { get; private set; }
		public string? UserType { get; private set; } // "Telegram" or "Admin"

		public void SetUserContext(string userId, string userType)
		{
			UserId = userId;
			UserType = userType;
		}
	}
}