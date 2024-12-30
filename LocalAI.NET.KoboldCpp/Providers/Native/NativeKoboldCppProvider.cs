using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using LocalAI.NET.KoboldCpp.Models;
using LocalAI.NET.KoboldCpp.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace LocalAI.NET.KoboldCpp.Providers.Native
{
    public class NativeKoboldCppProvider : INativeKoboldCppProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly JsonSerializerSettings? _jsonSettings;
        private bool _disposed;

        public NativeKoboldCppProvider(HttpClient httpClient, ILogger? logger = null, JsonSerializerSettings? jsonSettings = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
            _jsonSettings = jsonSettings;
        }

        public async Task<KoboldCppResponse> GenerateAsync(
            KoboldCppRequest request,
            CancellationToken cancellationToken = default)
        {
            KoboldCppUtils.ValidateRequest(request);

            try
            {
                var requestJson = JsonConvert.SerializeObject(request, _jsonSettings);
                _logger?.LogDebug("Generation request: {Request}", requestJson);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/v1/generate", content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                
                _logger?.LogDebug("Generation response status: {Status}", response.StatusCode);
                _logger?.LogDebug("Generation raw response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogError("Non-success status code: {Status}", response.StatusCode);
                    throw new KoboldCppException(
                        "Generation request failed",
                        "KoboldCpp",
                        (int)response.StatusCode,
                        responseContent);
                }

                try {
                    var result = JsonConvert.DeserializeObject<KoboldCppResponse>(responseContent, _jsonSettings);
                    if (result == null)
                    {
                        _logger?.LogError("Deserialized response is null");
                        throw new KoboldCppException(
                            "Null response after deserialization",
                            "KoboldCpp",
                            null,
                            responseContent);
                    }
                    
                    if (result.Results == null || !result.Results.Any())
                    {
                        _logger?.LogError("Response has no results");
                        throw new KoboldCppException(
                            "No results in response",
                            "KoboldCpp",
                            null,
                            responseContent);
                    }

                    return result;
                }
                catch (JsonException ex)
                {
                    _logger?.LogError(ex, "Failed to deserialize response: {Response}", responseContent);
                    throw new KoboldCppException(
                        "Failed to deserialize response",
                        "KoboldCpp",
                        null,
                        $"Response: {responseContent}, Error: {ex.Message}");
                }
            }
            catch (Exception ex) when (ex is not KoboldCppException)
            {
                _logger?.LogError(ex, "Failed to generate response");
                throw new KoboldCppException("Failed to generate response", "KoboldCpp", null, ex.Message);
            }
        }

        public async IAsyncEnumerable<string> GenerateStreamAsync(
            KoboldCppRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;
            var content = new StringContent(
                JsonConvert.SerializeObject(request, _jsonSettings),
                Encoding.UTF8,
                "application/json");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/extra/generate/stream")
            {
                Content = content
            };

            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var httpResponse = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    await Task.Delay(50, cancellationToken);
                    continue;
                }

                _logger?.LogDebug("Received line: {Line}", line);

                if (!line.StartsWith("data: ")) continue;

                var data = line[6..];
                if (data == "[DONE]") break;

                string? token = null;
                try
                {
                    var streamResponse = JsonConvert.DeserializeObject<StreamResponse>(data, _jsonSettings);
                    token = streamResponse?.Token;
                }
                catch (JsonException ex)
                {
                    _logger?.LogWarning(ex, "Failed to parse SSE message: {Message}", data);
                    continue;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    _logger?.LogDebug("Yielding token: {Token}", token);
                    yield return token;
                }
            }
        }

        public async Task<KoboldCppModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new KoboldCppException(
                        "Failed to get model info",
                        "KoboldCpp",
                        (int)response.StatusCode,
                        content);
                }

                return JsonConvert.DeserializeObject<KoboldCppModelInfo>(content, _jsonSettings)
                    ?? throw new KoboldCppException("Failed to deserialize model info");
            }
            catch (Exception ex) when (ex is not KoboldCppException)
            {
                throw new KoboldCppException("Failed to get model info", ex);
            }
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private class StreamResponse
        {
            [JsonProperty("token")]
            public string Token { get; set; } = string.Empty;

            [JsonProperty("finish_reason")] 
            public string? FinishReason { get; set; }
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