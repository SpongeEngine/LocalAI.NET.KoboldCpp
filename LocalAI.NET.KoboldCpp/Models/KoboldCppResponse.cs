using Newtonsoft.Json;

namespace LocalAI.NET.KoboldCpp.Models
{
    public class KoboldCppResponse
    {
        [JsonProperty("results")]
        public List<KoboldCppResult> Results { get; set; } = new();
    }

    public class KoboldCppResult
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
}