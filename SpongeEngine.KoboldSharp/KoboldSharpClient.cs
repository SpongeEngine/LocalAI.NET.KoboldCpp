using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.LLMSharp.Core;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient : LlmClientBase
    {
        public override KoboldSharpClientOptions Options { get; }
        
        public KoboldSharpClient(KoboldSharpClientOptions options): base(options)
        {
            Options = options;
        }
    }
}