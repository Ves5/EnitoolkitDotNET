using System.Text.Json.Serialization;

namespace Enitoolkit.Models
{
    public class DictionaryImportItem
    {
        [JsonPropertyName("letters")]
        public required Dictionary<char, int> Letters { get; set; }
        [JsonPropertyName("words")]
        public required List<string> Words { get; set; }
    }
}
