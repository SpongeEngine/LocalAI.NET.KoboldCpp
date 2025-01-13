using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.LLMSharp.Core.Base;
using SpongeEngine.LLMSharp.Core.Configuration;
using SpongeEngine.LLMSharp.Core.Models;

namespace SpongeEngine.KoboldSharp.Providers.KoboldSharpOpenAI
{
    public class KoboldSharpOpenAiProvider: BaseLlmProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly string _modelName;
        private bool _disposed;

        public KoboldSharpOpenAiProvider(
            HttpClient httpClient, 
            LlmOptions llmOptions, string name, string baseUrl, string modelName = "koboldcpp",
            ILogger? logger = null): base(httpClient, llmOptions, logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            Name = name;
            BaseUrl = baseUrl;
            _modelName = modelName;
            _logger = logger;
        }

        public async Task<string> CompleteAsync(
            string prompt,
            CompletionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new
            {
                model = options?.ModelName ?? _modelName,
                prompt = prompt,
                max_tokens = options?.MaxTokens ?? 80,
                temperature = options?.Temperature ?? 0.7f,
                top_p = options?.TopP ?? 0.9f,
                stop = options?.StopSequences,
                stream = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("v1/completions", content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new KoboldSharpException(
                    "OpenAI completion request failed",
                    "KoboldCpp",
                    (int)response.StatusCode,
                    responseContent);
            }

            var result = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);
            return result?.Choices.FirstOrDefault()?.Text ?? string.Empty;
        }

        public async IAsyncEnumerable<string> StreamCompletionAsync(
            string prompt,
            CompletionOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var request = new
            {
                model = options?.ModelName ?? _modelName,
                prompt = prompt,
                max_tokens = options?.MaxTokens ?? 80,
                temperature = options?.Temperature ?? 0.7f,
                top_p = options?.TopP ?? 0.9f,
                stop = options?.StopSequences,
                stream = true
            };

            var requestJson = JsonSerializer.Serialize(request);
            _logger?.LogDebug("OpenAI streaming request: {Payload}", requestJson);

            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/completions")
            {
                Content = content
            };

            using var response = await _httpClient.SendAsync(httpRequest, 
                HttpCompletionOption.ResponseHeadersRead, 
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6);
                if (data == "[DONE]")
                    break;

                StreamCompletion completion = null;
                string text = null;

                try 
                {
                    completion = JsonSerializer.Deserialize<StreamCompletion>(data);
                    text = completion?.Choices?.FirstOrDefault()?.Text;
                }
                catch (JsonException ex)
                {
                    _logger?.LogWarning(ex, "Failed to parse SSE message: {Message}", data);
                }

                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
        
        private class StreamCompletion
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }

            public class Choice
            {
                [JsonPropertyName("text")] 
                public string Text { get; set; }
            }
        }

        public override string Name { get; }
        public override string? Version { get; }
        public override bool SupportsStreaming { get; }
        public override string BaseUrl { get; }
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("v1/models", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public override Task<IDictionary<string, object>> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private class OpenAiResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();

            public class Choice
            {
                [JsonPropertyName("text")]
                public string Text { get; set; } = string.Empty;
            }
        }

        private class OpenAiStreamResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; } = new();

            public class Choice
            {
                [JsonPropertyName("text")]
                public string Text { get; set; } = string.Empty;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Don't dispose the HttpClient as it was injected
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