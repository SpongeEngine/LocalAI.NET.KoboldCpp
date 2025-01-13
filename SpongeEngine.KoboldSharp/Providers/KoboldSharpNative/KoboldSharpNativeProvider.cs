using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.LLMSharp.Core.Base;
using SpongeEngine.LLMSharp.Core.Configuration;
using JsonException = Newtonsoft.Json.JsonException;

namespace SpongeEngine.KoboldSharp.Providers.KoboldSharpNative
{
    public class KoboldSharpNativeProvider : BaseLlmProvider
    {
        public KoboldSharpNativeProvider(HttpClient httpClient, LlmOptions options, string name, string baseUrl, ILogger? logger = null) : base(httpClient, options, logger)
        {
            Name = name;
            BaseUrl = baseUrl;
        }

        public async Task<KoboldSharpResponse> GenerateAsync(
            KoboldSharpRequest request,
            CancellationToken cancellationToken = default)
        {
            //KoboldCppUtils.ValidateRequest(request);

            try
            {
                var requestJson = JsonConvert.SerializeObject(request);
                _logger?.LogDebug("Generation request: {Request}", requestJson);

                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/v1/generate", content, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                
                _logger?.LogDebug("Generation response status: {Status}", response.StatusCode);
                _logger?.LogDebug("Generation raw response: {Response}", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger?.LogError("Non-success status code: {Status}", response.StatusCode);
                    throw new KoboldSharpException(
                        "Generation request failed",
                        "KoboldCpp",
                        (int)response.StatusCode,
                        responseContent);
                }

                try {
                    var result = JsonConvert.DeserializeObject<KoboldSharpResponse>(responseContent);
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

        public async IAsyncEnumerable<string> GenerateStreamAsync(
            KoboldSharpRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            request.Stream = true;
            var content = new StringContent(
                JsonConvert.SerializeObject(request),
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
                    var streamResponse = JsonConvert.DeserializeObject<StreamResponse>(data);
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
                var response = await _httpClient.GetAsync("api/v1/model", cancellationToken);
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new KoboldSharpException(
                        "Failed to get model info",
                        "KoboldCpp",
                        (int)response.StatusCode,
                        content);
                }

                return JsonConvert.DeserializeObject<KoboldSharpModelInfo>(content)
                    ?? throw new KoboldSharpException("Failed to deserialize model info");
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