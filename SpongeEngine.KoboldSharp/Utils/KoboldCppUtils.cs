using System.Text;
using SpongeEngine.KoboldSharp.Models;

namespace SpongeEngine.KoboldSharp.Utils
{
    public static class KoboldCppUtils
    {
        /// <summary>
        /// Creates a KoboldCpp request with common default settings
        /// </summary>
        public static KoboldSharpRequest CreateDefaultRequest(
            string prompt,
            int maxLength = 80,
            float temperature = 0.7f,
            float topP = 0.9f)
        {
            return new KoboldSharpRequest
            {
                Prompt = prompt,
                MaxLength = maxLength,
                Temperature = temperature,
                TopP = topP,
                TrimStop = true
            };
        }

        /// <summary>
        /// Generates a unique request ID for tracking streaming responses
        /// </summary>
        public static string GenerateRequestId() =>
            $"kcpp_{Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()))}";

        /// <summary>
        /// Validates a KoboldCpp request, throwing if invalid
        /// </summary>
        public static void ValidateRequest(KoboldSharpRequest request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                throw new KoboldSharpException("Prompt cannot be empty");

            if (request.MaxLength <= 0)
                throw new KoboldSharpException("Max length must be greater than 0");

            if (request.Temperature < 0)
                throw new KoboldSharpException("Temperature must be non-negative");

            if (request.TopP is < 0 or > 1)
                throw new KoboldSharpException("Top P must be between 0 and 1");
        }
    }
}