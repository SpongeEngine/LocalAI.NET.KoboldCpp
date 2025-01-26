using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class InterrogateRequest
        {
            [JsonPropertyName("image")]
            public string Image { get; set; } = string.Empty;
        }

        public class InterrogateResponse
        {
            [JsonPropertyName("caption")]
            public string Caption { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Interrogates an image to generate a caption.
        /// </summary>
        public async Task<InterrogateResponse> InterrogateImageAsync(InterrogateRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "sdapi/v1/interrogate");
                httpRequest.Content = JsonContent.Create(request);

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to interrogate image",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<InterrogateResponse>(content) ?? throw new LlmSharpException("Failed to deserialize interrogate response");
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to interrogate image", innerException: ex);
            }
        }
    }
}