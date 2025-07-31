using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NihongoBot.Domain;

namespace NihongoBot.Persistence.Configurations;

public class ImageCacheConfiguration : IEntityTypeConfiguration<ImageCache>
{
	public void Configure(EntityTypeBuilder<ImageCache> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.Character)
			.IsRequired()
			.HasMaxLength(10); // Japanese characters can be up to 4 bytes in UTF-8, allowing for compound characters

		builder.Property(x => x.CacheKey)
			.IsRequired()
			.HasMaxLength(16); // SHA256 hash truncated to 16 characters

		builder.Property(x => x.ImageData)
			.IsRequired()
			.HasColumnType("bytea"); // PostgreSQL binary data type

		builder.Property(x => x.AccessCount)
			.IsRequired()
			.HasDefaultValue(0);

		builder.Property(x => x.LastAccessedAt)
			.IsRequired(false);

		// Create unique index on CacheKey for efficient lookups
		builder.HasIndex(x => x.CacheKey)
			.IsUnique()
			.HasDatabaseName("IX_ImageCache_CacheKey");

		// Create index on Character for lookups by character
		builder.HasIndex(x => x.Character)
			.IsUnique()
			.HasDatabaseName("IX_ImageCache_Character");

		// Create index on UpdatedAt for efficient cleanup of expired entries
		builder.HasIndex(x => x.UpdatedAt)
			.HasDatabaseName("IX_ImageCache_UpdatedAt");

		builder.ToTable("ImageCache");
	}
}