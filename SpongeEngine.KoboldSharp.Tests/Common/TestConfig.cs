namespace SpongeEngine.KoboldSharp.Tests.Common
{
    public static class TestConfig
    {
        private const string DefaultHost = "http://localhost:5001";

        public static string NativeApiBaseUrl => 
            Environment.GetEnvironmentVariable("KOBOLDCPP_BASE_URL") ?? $"{DefaultHost}/api";

        public static string OpenAiApiBaseUrl => 
            Environment.GetEnvironmentVariable("KOBOLDCPP_OPENAI_BASE_URL") ?? $"{DefaultHost}/v1";
            
        // Extended timeout for large models
        public static int TimeoutSeconds => 120;
    }
}