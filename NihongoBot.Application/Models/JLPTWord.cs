    namespace NihongoBot.Application.Models;
	public class JLPTWord
    {
        public required string Word { get; set; }
        public required string Meaning { get; set; }
        public required string Furigana { get; set; }
        public required string Romaji { get; set; }
        public int Level { get; set; }
    }
