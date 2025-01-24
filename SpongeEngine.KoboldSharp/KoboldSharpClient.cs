using System.Runtime.CompilerServices;
using SpongeEngine.LLMSharp.Core;
using SpongeEngine.LLMSharp.Core.Interfaces;
using SpongeEngine.LLMSharp.Core.Models;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient : LlmClientBase, ICompletionService
    {
        public override KoboldSharpClientOptions Options { get; }
        
        public KoboldSharpClient(KoboldSharpClientOptions options): base(options)
        {
            Options = options;
        }

        public async Task<CompletionResult> CompleteAsync(CompletionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            var koboldRequest = new KoboldSharpRequest
            {
                Prompt = request.Prompt,
                MaxLength = request.MaxTokens ?? 80,
                Temperature = request.Temperature,
                TopP = request.TopP,
                StopSequences = request.StopSequences.ToList(),
                Stream = false
            };

            var startTime = DateTime.UtcNow;
            var response = await GenerateAsync(koboldRequest, cancellationToken);

            // Get token counts using KoboldCpp's token counting API
            var promptTokens = await CountTokensAsync(new CountTokensRequest { Prompt = request.Prompt }, cancellationToken);
            var responseTokens = await CountTokensAsync(new CountTokensRequest { Prompt = response.Results[0].Text }, cancellationToken);

            return new CompletionResult
            {
                Text = response.Results[0].Text,
                ModelId = request.ModelId,
                GenerationTime = DateTime.UtcNow - startTime,
                TokenUsage = new CompletionTokenUsage
                {
                    PromptTokens = promptTokens.Count,
                    CompletionTokens = responseTokens.Count,
                    TotalTokens = promptTokens.Count + responseTokens.Count
                }
            };
        }

        public async IAsyncEnumerable<CompletionToken> StreamCompletionAsync(CompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = new CancellationToken())
        {
            var koboldRequest = new KoboldSharpRequest
            {
                Prompt = request.Prompt,
                MaxLength = request.MaxTokens ?? 80,
                Temperature = request.Temperature,
                TopP = request.TopP,
                StopSequences = request.StopSequences.ToList(),
                Stream = true
            };

            await foreach (var token in GenerateStreamAsync(koboldRequest, cancellationToken))
            {
                var tokenCount = await CountTokensAsync(new CountTokensRequest { Prompt = token }, cancellationToken);
        
                yield return new CompletionToken
                {
                    Text = token,
                    TokenCount = tokenCount.Count
                };
            }
        }
    }
}