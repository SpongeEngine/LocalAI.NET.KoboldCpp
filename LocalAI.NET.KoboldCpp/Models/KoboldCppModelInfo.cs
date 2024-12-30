using Newtonsoft.Json;

namespace LocalAI.NET.KoboldCpp.Models
{
    public class KoboldCppModelInfo
    {
        [JsonProperty("result")]
        public string ModelName { get; set; } = string.Empty;
    }
}