using System.Text.Json.Serialization;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpResponse
    {
        [JsonPropertyName("results")]
        public List<KoboldSharpResult> Results { get; set; } = new();
    }

    public class KoboldSharpResult
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}