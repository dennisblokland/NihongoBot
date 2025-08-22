using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using NihongoBot.Domain;

using TimeZoneConverter;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");
		builder.HasKey(u => u.Id);
		builder.Property(u => u.TelegramId).IsRequired();
		builder.HasIndex(u => u.TelegramId).IsUnique();

		builder.Property(u => u.Username).IsRequired(false);
		builder.Property(u => u.Streak).HasDefaultValue(0);
		builder.Property(u => u.QuestionsPerDay).HasDefaultValue(2);
		builder.Property(u => u.WordOfTheDayEnabled).HasDefaultValue(true);

		// Converter: TimeZoneInfo <-> IANA string
		ValueConverter<TimeZoneInfo, string> tzConverter = new(
			toProvider => ToIanaForStorage(toProvider),
			fromProvider => FromIanaForRuntime(fromProvider));

		ValueComparer<TimeZoneInfo> tzComparer = new(
			(a, b) => a != null && b != null && a.Id == b.Id,
			tz => tz != null ? tz.Id.GetHashCode() : 0,
			tz => tz); // immutable enough for our purposes

		builder
			.Property(e => e.TimeZone)
			.HasConversion(tzConverter)
			.Metadata.SetValueComparer(tzComparer);

		builder
			.Property(e => e.TimeZone)
			.HasMaxLength(64); // column size
	}

	static string ToIanaForStorage(TimeZoneInfo tz)
	{
		if (tz == null) throw new ArgumentNullException(nameof(tz));

		// On Linux/macOS, tz.Id is already IANA; on Windows it’s a Windows ID.
		// Always store IANA.
		if (OperatingSystem.IsWindows())
			return TZConvert.WindowsToIana(tz.Id);
		return tz.Id;
	}

	static TimeZoneInfo FromIanaForRuntime(string ianaId)
	{
		if (string.IsNullOrWhiteSpace(ianaId))
			return TimeZoneInfo.Utc;

		if (OperatingSystem.IsWindows())
		{
			string win = TZConvert.IanaToWindows(ianaId);
			return TimeZoneInfo.FindSystemTimeZoneById(win);
		}
		return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
	}
}
