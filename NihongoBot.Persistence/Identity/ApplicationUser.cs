using Microsoft.AspNetCore.Identity;

namespace NihongoBot.Persistence.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
	public Guid WebUserId { get; set; }
}