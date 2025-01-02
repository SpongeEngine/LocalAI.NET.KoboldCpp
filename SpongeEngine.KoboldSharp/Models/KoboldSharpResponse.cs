using Newtonsoft.Json;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpResponse
    {
        [JsonProperty("results")]
        public List<KoboldSharpResult> Results { get; set; } = new();
    }

    public class KoboldSharpResult
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
}