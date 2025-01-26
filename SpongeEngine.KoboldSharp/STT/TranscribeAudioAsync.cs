using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public class WhisperRequest
        {
            [JsonPropertyName("audio_data")]
            public string AudioData { get; set; } = string.Empty;

            [JsonPropertyName("prompt")]
            public string Prompt { get; set; } = string.Empty;

            [JsonPropertyName("suppress_non_speech")]
            public bool SuppressNonSpeech { get; set; } = false;
        }

        public class WhisperResponse
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }
        
        /// <summary>
        /// Transcribes audio to text using Whisper.
        /// </summary>
        public async Task<WhisperResponse> TranscribeAudioAsync(WhisperRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/transcribe");
                httpRequest.Content = JsonContent.Create(request);

                using HttpResponseMessage response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to transcribe audio",
                        (int)response.StatusCode,
                        content);
                }

                return JsonSerializer.Deserialize<WhisperResponse>(content) ??
                       throw new LlmSharpException("Failed to deserialize transcribe response");
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to transcribe audio", innerException: ex);
            }
        }
    }
}