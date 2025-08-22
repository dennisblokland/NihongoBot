namespace NihongoBot.Shared.Options;

public class AdminSettings
{
	public const string SectionKey = "AdminSettings";
	
	public string DefaultAdminEmail { get; set; } = "admin@nihongobot.local";
	public string DefaultAdminUsername { get; set; } = "admin";
	public string DefaultAdminPassword { get; set; } = "Admin123!";
	public string DefaultAdminFirstName { get; set; } = "Admin";
	public string DefaultAdminLastName { get; set; } = "User";
}