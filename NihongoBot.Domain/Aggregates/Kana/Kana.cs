using NihongoBot.Domain.Base;
using NihongoBot.Domain.Enums;

namespace NihongoBot.Domain.Aggregates.Kana
{
    public class Kana : DomainEntity<int>
    {
        public required string Character { get; set; }
        public required string Romaji { get; set; }
        public List<KanaVariant> Variants { get; set; } = [];

        public KanaType Type { get; set; }
    }
}
