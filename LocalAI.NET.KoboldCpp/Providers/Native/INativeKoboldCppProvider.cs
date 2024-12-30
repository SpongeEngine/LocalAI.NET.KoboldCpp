using LocalAI.NET.KoboldCpp.Models;

namespace LocalAI.NET.KoboldCpp.Providers.Native
{
    public interface INativeKoboldCppProvider : IDisposable
    {
        Task<KoboldCppResponse> GenerateAsync(KoboldCppRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> GenerateStreamAsync(KoboldCppRequest request, CancellationToken cancellationToken = default);
        Task<KoboldCppModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}