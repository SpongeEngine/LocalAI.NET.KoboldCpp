namespace SpongeEngine.KoboldSharp.Tests.Common
{
    public static class TestConfig
    {
        public const string BaseUrl = "http://localhost:5001";

        public static string NativeApiBaseUrl => Environment.GetEnvironmentVariable("KOBOLDCPP_BASE_URL") ?? $"{BaseUrl}/api";

        public static string OpenAiApiBaseUrl => Environment.GetEnvironmentVariable("KOBOLDCPP_OPENAI_BASE_URL") ?? $"{BaseUrl}/v1";
            
        // Extended timeout for large models
        public static int TimeoutSeconds => 120;
    }
}