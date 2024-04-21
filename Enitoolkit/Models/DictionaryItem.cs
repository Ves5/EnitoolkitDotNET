namespace Enitoolkit.Models
{
    public class DictionaryItem
    {
        public required string Key { get; set; }
        public required int Length { get; set; }
        public required Dictionary<char, int> Letters { get; set; }
        public required List<string> Words { get; set; }
    }
}
