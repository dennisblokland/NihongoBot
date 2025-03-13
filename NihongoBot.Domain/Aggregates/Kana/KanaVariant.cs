using NihongoBot.Domain.Base;

namespace NihongoBot.Domain.Aggregates.Hiragana
{
	public class KanaVariant
    {
        public string Character { get; set; }
        public string Romaji { get; set; }
		public int KanaId { get; set; }
	}
}
