using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NihongoBot.Domain.Aggregates.Hiragana;
using NihongoBot.Domain.Enums;

using System.Text.Json;

namespace NihongoBot.Persistence.Configurations
{
    public class KanaConfiguration : IEntityTypeConfiguration<Kana>
    {
        public void Configure(EntityTypeBuilder<Kana> builder)
        {
            builder.HasKey(k => k.Id);
            builder.Property(u => u.Character).IsRequired();
            builder.HasIndex(u => u.Character).IsUnique();
            builder.Property(k => k.Character).IsRequired().HasMaxLength(2);
            builder.Property(k => k.Romaji).IsRequired().HasMaxLength(5);
            builder.Property(k => k.Type).HasConversion<int>();


            List<Kana> kanaData = LoadKanaData();
            List<KanaVariant> kanaVariants = kanaData.SelectMany(k => k.Variants).ToList();
            kanaData.ForEach(k => k.Variants = []);
            builder.HasData(kanaData);

            builder.OwnsMany(k => k.Variants, variant =>
            {
                variant.HasKey(v => v.Character);
                variant.WithOwner().HasForeignKey(k => k.KanaId);
                variant.Property(v => v.Character).IsRequired().HasMaxLength(2);
                variant.Property(v => v.Romaji).IsRequired().HasMaxLength(5);
                variant.HasData(kanaVariants);
            });
        }

        private List<Kana> LoadKanaData()
        {
            string jsonFilePath = "hiragana.json";
			string fulljsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFilePath);

            if (!File.Exists(fulljsonFilePath))
            {
                Console.WriteLine($"Error: File '{fulljsonFilePath}' not found.");
                return [];
            }

            string jsonData = File.ReadAllText(fulljsonFilePath);
            List<Kana>? kanaList = JsonSerializer.Deserialize<List<Kana>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            foreach (Kana kana in kanaList)
            {
                kana.Type = KanaType.Hiragana;
                foreach (KanaVariant variant in kana.Variants)
                {
                    variant.KanaId = kana.Id;
                }
            }

            return kanaList;
        }

    }
}
