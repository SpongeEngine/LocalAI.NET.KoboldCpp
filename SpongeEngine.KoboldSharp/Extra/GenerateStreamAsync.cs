using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SpongeEngine.LLMSharp.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
                /// <summary>
        /// Generates text given a prompt and generation settings, with SSE streaming. Unspecified values are set to defaults.
        /// SSE streaming establishes a persistent connection, returning ongoing process in the form of message events.
        /// event: message
        /// data: {data}
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="customJsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="LlmSharpException"></exception>
        public async IAsyncEnumerable<string> GenerateStreamAsync(KoboldSharpClient.KoboldSharpRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default, JsonSerializerOptions? customJsonSerializerOptions = null)
        {
            request.Stream = true;
            
            JsonSerializerOptions jsonSerializerOptions = customJsonSerializerOptions ?? Options.JsonSerializerOptions;

            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/extra/generate/stream");
            httpRequest.Content = JsonContent.Create(request, options: jsonSerializerOptions);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using HttpResponseMessage? httpResponse = await Options.HttpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            string responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                Options.Logger?.LogError("Non-success status code: {Status}", httpResponse.StatusCode);
                throw new LlmSharpException(
                    "Generation request failed",
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

                Options.Logger?.LogDebug("Received line: {Line}", line);

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
                    Options.Logger?.LogWarning(ex, "Failed to parse SSE message: {Message}", data);
                    continue;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    Options.Logger?.LogDebug("Yielding token: {Token}", token);
                    yield return token;
                }
            }
        }
        
        private class StreamResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("finish_reason")] 
            public string? FinishReason { get; set; }
        }
    }
}