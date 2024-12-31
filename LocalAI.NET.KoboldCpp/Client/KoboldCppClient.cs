using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using LocalAI.NET.KoboldCpp.Models;
using LocalAI.NET.KoboldCpp.Providers.Native;
using LocalAI.NET.KoboldCpp.Providers.OpenAiCompatible;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LocalAI.NET.KoboldCpp.Client
{
    public class KoboldCppClient : IDisposable
    {
        private readonly INativeKoboldCppProvider? _nativeProvider;
        private readonly IOpenAiKoboldCppProvider? _openAiProvider;
        private readonly KoboldCppOptions _options;
        private bool _disposed;

        public string Name => "KoboldCpp";
        public string? Version { get; private set; }
        public bool SupportsStreaming => true;

        public KoboldCppClient(KoboldCppOptions options, ILogger? logger = null, JsonSerializerSettings? jsonSettings = null)
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
                _openAiProvider = new OpenAiKoboldCppProvider(httpClient, logger: logger, jsonSettings: jsonSettings);
            }
            else
            {
                _nativeProvider = new NativeKoboldCppProvider(httpClient, logger: logger, jsonSettings: jsonSettings);
            }
        }

        // Native API methods
        public Task<KoboldCppResponse> GenerateAsync(KoboldCppRequest request, CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GenerateAsync(request, cancellationToken);
        }

        public IAsyncEnumerable<string> GenerateStreamAsync(KoboldCppRequest request, CancellationToken cancellationToken = default)
        {
            EnsureNativeProvider();
            return _nativeProvider!.GenerateStreamAsync(request, cancellationToken);
        }

        public Task<KoboldCppModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
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