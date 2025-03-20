using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NihongoBot.Domain;

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
    }
}
