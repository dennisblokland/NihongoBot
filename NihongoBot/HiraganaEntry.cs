public class HiraganaEntry
{
    public string Character { get; set; } = string.Empty;
    public string Romaji { get; set; } = string.Empty;
    public List<HiraganaEntry>? Variants { get; set; } // Stores full variant entries
}
