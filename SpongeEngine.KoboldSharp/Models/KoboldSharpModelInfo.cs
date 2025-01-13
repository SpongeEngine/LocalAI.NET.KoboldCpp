using System.Text.Json.Serialization;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpModelInfo
    {
        [JsonPropertyName("result")]
        public string ModelName { get; set; } = string.Empty;
    }
}