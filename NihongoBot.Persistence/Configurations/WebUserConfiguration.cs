using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NihongoBot.Domain.Aggregates.WebUser;

namespace NihongoBot.Persistence.Configurations;

public class WebUserConfiguration : IEntityTypeConfiguration<WebUser>
{
	public void Configure(EntityTypeBuilder<WebUser> builder)
	{
		builder.ToTable("WebUsers");
		builder.HasKey(u => u.Id);
		
		builder.Property(u => u.Email)
			.IsRequired()
			.HasMaxLength(255);
		
		builder.Property(u => u.Username)
			.IsRequired()
			.HasMaxLength(100);
		
		builder.Property(u => u.FirstName)
			.IsRequired()
			.HasMaxLength(100);
		
		builder.Property(u => u.LastName)
			.IsRequired()
			.HasMaxLength(100);
		
		builder.Property(u => u.IsEnabled)
			.IsRequired()
			.HasDefaultValue(true);
		
		builder.Property(u => u.LastLoginAt)
			.IsRequired(false);
		
		builder.HasIndex(u => u.Email).IsUnique();
		builder.HasIndex(u => u.Username).IsUnique();
	}
}