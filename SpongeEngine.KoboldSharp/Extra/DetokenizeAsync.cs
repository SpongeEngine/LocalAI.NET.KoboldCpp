using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.LLMSharp.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class DetokenizeRequest
        {
            [JsonPropertyName("ids")] 
            public List<int> TokenIds { get; set; } = new();
        }

        public class DetokenizeResponse
        {
            [JsonPropertyName("result")]
            public string Text { get; set; } = string.Empty;

            [JsonPropertyName("success")]
            public bool Success { get; set; }
        }
        
        /// <summary>
        /// Converts token IDs back into text.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="LlmSharpException"></exception>
        public async Task<DetokenizeResponse> DetokenizeAsync(DetokenizeRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/detokenize");
                httpRequest.Content = JsonContent.Create(request);
                
                using HttpResponseMessage? response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to detokenize",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<DetokenizeResponse>(content) ?? throw new LlmSharpException("Failed to deserialize detokenize response");
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to detokenize", innerException: ex);
            }
        }
    }
}