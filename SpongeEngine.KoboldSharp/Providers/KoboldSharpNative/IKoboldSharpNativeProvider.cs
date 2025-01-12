using SpongeEngine.KoboldSharp.Models;

namespace SpongeEngine.KoboldSharp.Providers.KoboldSharpNative
{
    public interface IKoboldSharpNativeProvider : IDisposable
    {
        Task<KoboldSharpResponse> GenerateAsync(KoboldSharpRequest request, CancellationToken cancellationToken = default);
        IAsyncEnumerable<string> GenerateStreamAsync(KoboldSharpRequest request, CancellationToken cancellationToken = default);
        Task<KoboldSharpModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
    }
}