using Newtonsoft.Json;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpRequest
    {
        [JsonProperty("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonProperty("max_length")]
        public int MaxLength { get; set; } = 80;

        [JsonProperty("max_context_length")]
        public int? MaxContextLength { get; set; }

        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.7f;

        [JsonProperty("top_p")]
        public float TopP { get; set; } = 0.9f;

        [JsonProperty("top_k")]
        public int TopK { get; set; } = 40;

        [JsonProperty("top_a")]
        public float TopA { get; set; } = 0.0f;

        [JsonProperty("typical")]
        public float Typical { get; set; } = 1.0f;

        [JsonProperty("tfs")]
        public float Tfs { get; set; } = 1.0f;

        [JsonProperty("rep_pen")]
        public float RepetitionPenalty { get; set; } = 1.1f;

        [JsonProperty("rep_pen_range")]
        public int RepetitionPenaltyRange { get; set; } = 64;

        [JsonProperty("mirostat")]
        public int MirostatMode { get; set; } = 0;

        [JsonProperty("mirostat_tau")]
        public float MirostatTau { get; set; } = 5.0f;

        [JsonProperty("mirostat_eta")]
        public float MirostatEta { get; set; } = 0.1f;

        [JsonProperty("stop_sequence")]
        public List<string>? StopSequences { get; set; }

        [JsonProperty("stream")]
        public bool Stream { get; set; }

        [JsonProperty("trim_stop")]
        public bool TrimStop { get; set; } = true;

        [JsonProperty("grammar")]
        public string? Grammar { get; set; }

        [JsonProperty("memory")]
        public string? Memory { get; set; }

        [JsonProperty("banned_tokens")]
        public List<string>? BannedTokens { get; set; }

        [JsonProperty("logit_bias")]
        public Dictionary<string, float>? LogitBias { get; set; }
    }
}