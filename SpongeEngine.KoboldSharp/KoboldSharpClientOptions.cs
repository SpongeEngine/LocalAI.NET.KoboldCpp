using SpongeEngine.LLMSharp.Core;

namespace SpongeEngine.KoboldSharp.Models
{
    public class KoboldSharpClientOptions: LlmClientBaseOptions
    {
        #region LlmClientBaseOptions
        public override string BaseUrl { get; set; } = "http://localhost:5001";
        #endregion

        #region KoboldSharpClientOptions
        /// <summary>
        /// Whether to use GPU acceleration.
        /// </summary>
        public bool UseGpu { get; set; } = true;
        #endregion
    }
}