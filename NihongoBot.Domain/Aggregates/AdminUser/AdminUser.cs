using NihongoBot.Domain.Base;

namespace NihongoBot.Domain.Aggregates.AdminUser
{
	public class AdminUser : DomainEntity
	{
		public AdminUser(string email, string username)
		{
			Email = email;
			Username = username;
			IsEnabled = true;
		}

		public string Email { get; private set; }
		public string Username { get; private set; }
		public bool IsEnabled { get; private set; }
		public DateTime? LastLoginAt { get; private set; }

		public void UpdateEmail(string email)
		{
			Email = email;
		}

		public void UpdateUsername(string username)
		{
			Username = username;
		}

		public void Enable()
		{
			IsEnabled = true;
		}

		public void Disable()
		{
			IsEnabled = false;
		}

		public void UpdateLastLogin()
		{
			LastLoginAt = DateTime.UtcNow;
		}
	}
}