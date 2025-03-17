using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NihongoBot.Domain;
using NihongoBot.Domain.Entities;

namespace NihongoBot.Persistence.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.UserId)
                .IsRequired();

            builder.Property(q => q.QuestionText)
                .IsRequired();

            builder.Property(q => q.QuestionType)
                .IsRequired();

            builder.Property(q => q.CorrectAnswer)
                .IsRequired();

            builder.Property(q => q.SentAt)
                .IsRequired();

            builder.Property(q => q.IsAnswered)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(q => q.IsExpired)
                .IsRequired()
                .HasDefaultValue(false);

			builder.Property(q => q.Attempts)
				.HasDefaultValue(0);

            // Relationships
            builder.HasOne(q => q.User)
                .WithMany()
                .HasForeignKey(q => q.UserId);


        }
    }
}
