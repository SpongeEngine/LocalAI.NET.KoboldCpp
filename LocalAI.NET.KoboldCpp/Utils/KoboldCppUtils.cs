using System.Text;
using LocalAI.NET.KoboldCpp.Models;

namespace LocalAI.NET.KoboldCpp.Utils
{
    public static class KoboldCppUtils
    {
        /// <summary>
        /// Creates a KoboldCpp request with common default settings
        /// </summary>
        public static KoboldCppRequest CreateDefaultRequest(
            string prompt,
            int maxLength = 80,
            float temperature = 0.7f,
            float topP = 0.9f)
        {
            return new KoboldCppRequest
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
        public static void ValidateRequest(KoboldCppRequest request)
        {
            if (string.IsNullOrEmpty(request.Prompt))
                throw new KoboldCppException("Prompt cannot be empty");

            if (request.MaxLength <= 0)
                throw new KoboldCppException("Max length must be greater than 0");

            if (request.Temperature < 0)
                throw new KoboldCppException("Temperature must be non-negative");

            if (request.TopP is < 0 or > 1)
                throw new KoboldCppException("Top P must be between 0 and 1");
        }
    }
}