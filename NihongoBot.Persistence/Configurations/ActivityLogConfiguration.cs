using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NihongoBot.Domain.Aggregates.ActivityLog;

namespace NihongoBot.Persistence.Configurations
{
	public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
	{
		public void Configure(EntityTypeBuilder<ActivityLog> builder)
		{
			builder.ToTable("ActivityLogs");
			builder.HasKey(a => a.Id);
			
			builder.Property(a => a.Action)
				.IsRequired()
				.HasMaxLength(100);
			
			builder.Property(a => a.EntityType)
				.IsRequired()
				.HasMaxLength(100);
			
			builder.Property(a => a.EntityId)
				.IsRequired()
				.HasMaxLength(100);
			
			builder.Property(a => a.Details)
				.HasMaxLength(1000);
			
			builder.Property(a => a.UserId)
				.HasMaxLength(100);
			
			builder.Property(a => a.UserType)
				.HasMaxLength(50);
			
			builder.HasIndex(a => a.Timestamp);
			builder.HasIndex(a => new { a.EntityType, a.EntityId });
			builder.HasIndex(a => new { a.UserId, a.UserType });
		}
	}
}