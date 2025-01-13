using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.KoboldSharp.Providers.KoboldSharpNative;
using SpongeEngine.KoboldSharp.Providers.KoboldSharpOpenAI;
using SpongeEngine.LLMSharp.Core.Configuration;
using SpongeEngine.LLMSharp.Core.Models;

namespace SpongeEngine.KoboldSharp.Client
{
    public class KoboldSharpClient : IDisposable
    {
        private readonly KoboldSharpNativeProvider? _nativeProvider;
        private readonly KoboldSharpOpenAiProvider? _openAiProvider;
        private readonly KoboldSharpOptions _options;
        private bool _disposed;

        public string Name => "KoboldCpp";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public KoboldSharpClient(KoboldSharpOptions options, ILogger? logger = null, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(options.BaseUrl),
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds)
            };

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("text/event-stream"));

            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
            }

            if (options.UseOpenAiApi)
            {
                _openAiProvider = new KoboldSharpOpenAiProvider(httpClient, new LlmOptions(), Name, "", logger: logger);
            }
            else
            {
                _nativeProvider = new KoboldSharpNativeProvider(httpClient, new LlmOptions(), Name, "", logger);
            }
        }

        // Native API methods
        public Task<KoboldSharpResponse> GenerateAsync(KoboldSharpRequest request, CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GenerateAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<string> GenerateStreamAsync(KoboldSharpRequest request, CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GenerateStreamAsync(request, cancellationToken);
        }

        public Task<KoboldSharpModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GetModelInfoAsync(cancellationToken);
        }

        // OpenAI API methods
        public Task<string> CompleteAsync(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default)
        {
            EnsureOpenAiProvider();
            return _openAiProvider!.CompleteAsync(prompt, options, cancellationToken);
        }

        public IAsyncEnumerable<string> StreamCompletionAsync(string prompt, CompletionOptions? options = null, CancellationToken cancellationToken = default)
        {
            EnsureOpenAiProvider();
            return _openAiProvider!.StreamCompletionAsync(prompt, options, cancellationToken);
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            return _options.UseOpenAiApi 
                ? await _openAiProvider!.IsAvailableAsync(cancellationToken)
                : await _nativeProvider!.IsAvailableAsync(cancellationToken);
        }

        private void EnsureNativeProvider()
        {
            if (_nativeProvider == null)
                throw new InvalidOperationException("Native API is not enabled. Set UseOpenAiApi to false in options.");
        }

        private void EnsureOpenAiProvider()
        {
            if (_openAiProvider == null)
                throw new InvalidOperationException("OpenAI API is not enabled. Set UseOpenAiApi to true in options.");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _nativeProvider?.Dispose();
                    _openAiProvider?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}