using System.Text.Json.Serialization;

namespace Enitoolkit.Models
{
    public class MiscLink
    {
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("url")]
        public required string Url { get; set; }
    }
}
