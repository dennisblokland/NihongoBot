namespace NihongoBot.Shared.Options;
	public class ConnectionStrings : IConfigOptions
	{
		public const string SectionKey = "ConnectionStrings";
		public string NihongoBotDB { get; set; } = null!;
	}
