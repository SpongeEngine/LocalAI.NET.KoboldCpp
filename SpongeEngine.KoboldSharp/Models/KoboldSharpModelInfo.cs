using Newtonsoft.Json;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpModelInfo
    {
        [JsonProperty("result")]
        public string ModelName { get; set; } = string.Empty;
    }
}