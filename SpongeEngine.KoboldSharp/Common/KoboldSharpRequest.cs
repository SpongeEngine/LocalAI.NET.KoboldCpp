using System.Text.Json.Serialization;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class KoboldSharpRequest
        {
            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("max_length")]
            public int MaxLength { get; set; } = 80;

            [JsonPropertyName("max_context_length")]
            public int? MaxContextLength { get; set; }

            [JsonPropertyName("temperature")]
            public float Temperature { get; set; } = 0.7f;

            [JsonPropertyName("top_p")]
            public float TopP { get; set; } = 0.9f;

            [JsonPropertyName("top_k")]
            public int TopK { get; set; } = 40;

            [JsonPropertyName("top_a")]
            public float TopA { get; set; } = 0.0f;

            [JsonPropertyName("min_p")]
            public float MinP { get; set; } = 0.0f;

            [JsonPropertyName("typical")]
            public float Typical { get; set; } = 1.0f;

            [JsonPropertyName("tfs")]
            public float Tfs { get; set; } = 1.0f;

            [JsonPropertyName("rep_pen")]
            public float RepetitionPenalty { get; set; } = 1.1f;

            [JsonPropertyName("rep_pen_range")]
            public int RepetitionPenaltyRange { get; set; } = 320;

            [JsonPropertyName("rep_pen_slope")]
            public float RepetitionPenaltySlope { get; set; } = 1.0f;

            [JsonPropertyName("presence_penalty")]
            public float PresencePenalty { get; set; } = 0.0f;

            [JsonPropertyName("mirostat")]
            public int MirostatMode { get; set; } = 0;

            [JsonPropertyName("mirostat_tau")]
            public float MirostatTau { get; set; } = 5.0f;

            [JsonPropertyName("mirostat_eta")]
            public float MirostatEta { get; set; } = 0.1f;

            [JsonPropertyName("stop_sequence")]
            public List<string>? StopSequences { get; set; }

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }

            [JsonPropertyName("trim_stop")]
            public bool TrimStop { get; set; } = true;

            [JsonPropertyName("grammar")]
            public string? Grammar { get; set; }

            [JsonPropertyName("grammar_retain_state")]
            public bool GrammarRetainState { get; set; }

            [JsonPropertyName("memory")]
            public string? Memory { get; set; }

            [JsonPropertyName("banned_tokens")]
            public List<string>? BannedTokens { get; set; }

            [JsonPropertyName("logit_bias")]
            public Dictionary<string, float>? LogitBias { get; set; }

            [JsonPropertyName("images")]
            public List<string>? Images { get; set; }

            [JsonPropertyName("seed")]
            public int Seed { get; set; } = -1;

            [JsonPropertyName("quiet")]
            public bool Quiet { get; set; }

            [JsonPropertyName("allow_eos_token")]
            public bool AllowEosToken { get; set; } = true;

            [JsonPropertyName("bypass_eos_token")]
            public bool BypassEosToken { get; set; }

            [JsonPropertyName("render_special")]
            public bool RenderSpecial { get; set; }

            [JsonPropertyName("sampler_order")]
            public int[]? SamplerOrder { get; set; }

            [JsonPropertyName("dynatemp_range")]
            public float DynamicTemperatureRange { get; set; } = 0.0f;

            [JsonPropertyName("dynatemp_exponent")]
            public float DynamicTemperatureExponent { get; set; } = 1.0f;

            [JsonPropertyName("smoothing_factor")]
            public float SmoothingFactor { get; set; } = 0.0f;
        }
    }
}