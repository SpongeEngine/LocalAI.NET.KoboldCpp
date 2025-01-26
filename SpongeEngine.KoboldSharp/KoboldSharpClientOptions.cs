using SpongeEngine.SpongeLLM.Core;

namespace SpongeEngine.KoboldSharp
{
    public class KoboldSharpClientOptions : LLMClientBaseOptions 
    {
        // Client-side settings
        public bool MultiplayerEnabled { get; set; } = false;
        public bool WebSearchEnabled { get; set; } = false;
    
        // Stable Diffusion settings
        public string? StableDiffusionModelPath { get; set; }
        public string? StableDiffusionVaePath { get; set; }
        public bool StableDiffusionUseQuantization { get; set; } = false;
        public int StableDiffusionMaxResolution { get; set; } = 512;
        public int StableDiffusionThreads { get; set; } = -1;

        // Chat completion settings
        public string? ChatCompletionsAdapterPath { get; set; }

        // Progress tracking
        public bool ShowProgress { get; set; } = false;
        public int ProgressUpdateInterval { get; set; } = 100;
    }
}