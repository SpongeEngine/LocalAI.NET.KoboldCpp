using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.LLMSharp.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class StableDiffusionImageToImageRequest : StableDiffusionGenerationRequest
        {
            [JsonPropertyName("init_images")]
            public List<string> InitImages { get; set; } = new();

            [JsonPropertyName("denoising_strength")] 
            public float DenoisingStrength { get; set; } = 0.75f;
        }
        
        /// <summary>
        /// Generates an image from another image using Stable Diffusion.
        /// </summary>
        public async Task<StableDiffusionGenerationResponse> ImageToImageAsync(StableDiffusionImageToImageRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "sdapi/v1/img2img");
                httpRequest.Content = JsonContent.Create(request);

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to generate image from image",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<StableDiffusionGenerationResponse>(content) ?? throw new LlmSharpException("Failed to deserialize image generation response");
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to generate image from image", innerException: ex);
            }
        }
    }
}