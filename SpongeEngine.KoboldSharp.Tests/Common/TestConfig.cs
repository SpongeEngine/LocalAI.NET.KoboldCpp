namespace SpongeEngine.KoboldSharp.Tests.Common
{
    public static class TestConfig
    {
        public const string BaseUrl = "http://localhost:5001";

        public static string NativeApiBaseUrl => Environment.GetEnvironmentVariable("KOBOLDCPP_BASE_URL") ?? $"{BaseUrl}/api";
    }
}