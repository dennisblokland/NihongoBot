using NihongoBot.Domain.Base;

namespace NihongoBot.Domain.Aggregates.WebUser
{
	public class WebUser : DomainEntity
	{
		public WebUser(string email, string username, string firstName, string lastName)
		{
			Email = email;
			Username = username;
			FirstName = firstName;
			LastName = lastName;
			IsEnabled = true;
		}

		public string Email { get; private set; }
		public string Username { get; private set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public bool IsEnabled { get; private set; }
		public DateTime? LastLoginAt { get; private set; }

		public void UpdateProfile(string firstName, string lastName)
		{
			FirstName = firstName;
			LastName = lastName;
		}

		public void UpdateEmail(string email)
		{
			Email = email;
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