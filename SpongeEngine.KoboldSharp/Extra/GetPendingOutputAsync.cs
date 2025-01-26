using System.Net.Http.Json;
using System.Text.Json;
using SpongeEngine.SpongeLLM.Core.Exceptions;

namespace SpongeEngine.KoboldSharp
{
    public partial class KoboldSharpClient
    {
        public async Task<string> GetPendingOutputAsync(string? genKey = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpRequestMessage httpRequest = new(HttpMethod.Post, "api/extra/generate/check");
                if (!string.IsNullOrEmpty(genKey))
                {
                    httpRequest.Content = JsonContent.Create(new { genkey = genKey });
                }

                using HttpResponseMessage? response = await Options.HttpClient.SendAsync(httpRequest, cancellationToken);
                string? content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new LlmSharpException(
                        "Failed to get pending output",
                        (int)response.StatusCode,
                        content);
                }

                var result = JsonSerializer.Deserialize<GenerateAsyncResponse>(content);
                return result?.Results.FirstOrDefault()?.Text ?? string.Empty;
            }
            catch (Exception ex) when (ex is not LlmSharpException)
            {
                throw new LlmSharpException("Failed to get pending output", innerException: ex); 
            }
        }
    }
}