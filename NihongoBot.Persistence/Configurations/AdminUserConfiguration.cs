using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NihongoBot.Domain.Aggregates.AdminUser;

namespace NihongoBot.Persistence.Configurations
{
	public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
	{
		public void Configure(EntityTypeBuilder<AdminUser> builder)
		{
			builder.ToTable("AdminUsers");
			builder.HasKey(u => u.Id);
			
			builder.Property(u => u.Email)
				.IsRequired()
				.HasMaxLength(256);
			
			builder.Property(u => u.Username)
				.IsRequired()
				.HasMaxLength(256);
			
			builder.Property(u => u.IsEnabled)
				.HasDefaultValue(true);
			
			builder.HasIndex(u => u.Email).IsUnique();
			builder.HasIndex(u => u.Username).IsUnique();
		}
	}
}