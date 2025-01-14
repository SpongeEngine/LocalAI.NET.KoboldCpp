using SpongeEngine.LLMSharp.Core.Configuration;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpOptions: LlmClientOptions
    {
        /// <summary>
        /// HTTP request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 600;

        /// <summary>
        /// Maximum context size in tokens
        /// </summary>
        public int ContextSize { get; set; } = 2048;

        /// <summary>
        /// Whether to use GPU acceleration
        /// </summary>
        public bool UseGpu { get; set; } = true;
    }
}