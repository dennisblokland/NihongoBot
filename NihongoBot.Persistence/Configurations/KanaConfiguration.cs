using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NihongoBot.Domain.Aggregates.Kana;
using NihongoBot.Domain.Enums;

namespace NihongoBot.Persistence.Configurations
{
	public class KanaConfiguration : IEntityTypeConfiguration<Kana>
	{
		private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
		{
			PropertyNameCaseInsensitive = true
		};

		public void Configure(EntityTypeBuilder<Kana> builder)
		{
			_ = builder.HasKey(k => k.Id);
			_ = builder.Property(u => u.Character).IsRequired();
			_ = builder.HasIndex(u => u.Character).IsUnique();
			_ = builder.Property(k => k.Character).IsRequired().HasMaxLength(2);
			_ = builder.Property(k => k.Romaji).IsRequired().HasMaxLength(5);
			_ = builder.Property(k => k.Type).HasConversion<int>();


			List<Kana> kanaData = LoadKanaData();
			List<KanaVariant> kanaVariants = kanaData.SelectMany(k => k.Variants).ToList();
			kanaData.ForEach(k => k.Variants = []);
			_ = builder.HasData(kanaData);

			_ = builder.OwnsMany(k => k.Variants, variant =>
			{
				_ = variant.HasKey(v => v.Character);
				_ = variant.WithOwner().HasForeignKey(k => k.KanaId);
				_ = variant.Property(v => v.Character).IsRequired().HasMaxLength(2);
				_ = variant.Property(v => v.Romaji).IsRequired().HasMaxLength(5);
				_ = variant.HasData(kanaVariants);
			}
			);
		}

		private static List<Kana> LoadKanaData()
		{
			string jsonFilePath = "hiragana.json";
			if (!File.Exists(jsonFilePath))
			{
				Console.WriteLine($"Error: File '{jsonFilePath}' not found.");
				return [];
			}

			List<Kana>? kanaList = JsonSerializer.Deserialize<List<Kana>>(File.ReadAllText(jsonFilePath), _jsonSerializerOptions);

			kanaList?.ForEach(kana =>
			{
				kana.Type = KanaType.Hiragana;
				kana.Variants.ForEach(variant => variant.KanaId = kana.Id);
			});

			return kanaList ?? [];
		}
	}
}
