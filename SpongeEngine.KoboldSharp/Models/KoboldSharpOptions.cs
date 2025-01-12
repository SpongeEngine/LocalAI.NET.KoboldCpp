namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpOptions
    {
        /// <summary>
        /// Base URL of the KoboldCpp server
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:5001";

        /// <summary>
        /// Optional API key if authentication is enabled
        /// </summary>
        public string? ApiKey { get; set; }

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

        /// <summary>
        /// Whether to use the OpenAI-compatible API endpoints
        /// </summary>
        public bool UseOpenAiApi { get; set; }
    }
}