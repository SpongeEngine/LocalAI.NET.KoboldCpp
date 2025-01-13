using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.LLMSharp.Core.Base;
using SpongeEngine.LLMSharp.Core.Configuration;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SpongeEngine.KoboldSharp
{
    public class KoboldSharpClient : LlmClientBase
    {
        public KoboldSharpClient(HttpClient? httpClient, LlmClientOptions? llmOptions, string name, string baseUrl, ILogger? logger = null): base(httpClient ?? new HttpClient(), llmOptions ?? new LlmClientOptions(), logger)
        {
            Name = name;
            BaseUrl = baseUrl;
        }

        public async Task<KoboldSharpResponse> GenerateAsync(KoboldSharpRequest request, CancellationToken cancellationToken = default, JsonSerializerOptions? customJsonSerializerOptions = null)
        {
            //KoboldCppUtils.ValidateRequest(request);
            
            JsonSerializerOptions jsonSerializerOptions = customJsonSerializerOptions ?? LlmClientOptions.JsonSerializerOptions;

            try
            {
                using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v1/generate");
                httpRequest.Content = JsonContent.Create(request, options: jsonSerializerOptions);
                using HttpResponseMessage? httpResponse = await _httpClient.SendAsync(
                    httpRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                string responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                
                _logger?.LogDebug("Generation response status: {Status}", httpResponse.StatusCode);
                _logger?.LogDebug("Generation raw response: {Response}", responseContent);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger?.LogError("Non-success status code: {Status}", httpResponse.StatusCode);
                    throw new KoboldSharpException(
                        "Generation request failed",
                        "KoboldCpp",
                        (int)httpResponse.StatusCode,
                        responseContent);
                }

                try {
                    KoboldSharpResponse? result = JsonSerializer.Deserialize<KoboldSharpResponse>(responseContent, jsonSerializerOptions);
                    if (result == null)
                    {
                        _logger?.LogError("Deserialized response is null");
                        throw new KoboldSharpException(
                            "Null response after deserialization",
                            "KoboldCpp",
                            null,
                            responseContent);
                    }
                    
                    if (result.Results == null || !result.Results.Any())
                    {
                        _logger?.LogError("Response has no results");
                        throw new KoboldSharpException(
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
                    throw new KoboldSharpException(
                        "Failed to deserialize response",
                        "KoboldCpp",
                        null,
                        $"Response: {responseContent}, Error: {ex.Message}");
                }
            }
            catch (Exception ex) when (ex is not KoboldSharpException)
            {
                _logger?.LogError(ex, "Failed to generate response");
                throw new KoboldSharpException("Failed to generate response", "KoboldCpp", null, ex.Message);
            }
        }

        public async IAsyncEnumerable<string> GenerateStreamAsync(KoboldSharpRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default, JsonSerializerOptions? customJsonSerializerOptions = null)
        {
            request.Stream = true;
            
            JsonSerializerOptions jsonSerializerOptions = customJsonSerializerOptions ?? LlmClientOptions.JsonSerializerOptions;

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/extra/generate/stream");
            httpRequest.Content = JsonContent.Create(request, options: jsonSerializerOptions);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using HttpResponseMessage? httpResponse = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            string responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger?.LogError("Non-success status code: {Status}", httpResponse.StatusCode);
                throw new KoboldSharpException(
                    "Generation request failed",
                    "KoboldCpp",
                    (int)httpResponse.StatusCode,
                    responseContent);
            }

            using Stream? stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using StreamReader? reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                string? line = await reader.ReadLineAsync();
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
                    var streamResponse = JsonSerializer.Deserialize<StreamResponse>(data);
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

        public async Task<KoboldSharpModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new KoboldSharpException(
                        "Failed to get model info",
                        "KoboldCpp",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<KoboldSharpModelInfo>(content) ?? throw new KoboldSharpException("Failed to deserialize model info");
            }
            catch (Exception ex) when (ex is not KoboldSharpException)
            {
                throw new KoboldSharpException("Failed to get model info", ex);
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
                var response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
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

        private class StreamResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("finish_reason")] 
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