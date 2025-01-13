using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Providers.KoboldSharpOpenAI;
using SpongeEngine.KoboldSharp.Tests.Common;
using SpongeEngine.LLMSharp.Core.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Providers.OpenAiCompatible
{
    public abstract class OpenAiTestBase : IAsyncLifetime
    {
        protected readonly KoboldSharpOpenAiProvider Provider;
        protected readonly ITestOutputHelper Output;
        protected readonly ILogger Logger;
        protected bool ServerAvailable;

        protected OpenAiTestBase(ITestOutputHelper output)
        {
            Output = output;
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());

            var httpClient = new HttpClient 
            { 
                BaseAddress = new Uri(TestConfig.OpenAiApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(TestConfig.TimeoutSeconds)
            };

            Provider = new KoboldSharpOpenAiProvider(
                httpClient,
                new LlmOptions(),
                "KoboldCpp",
                "",
                logger: Logger);
        }

        public async Task InitializeAsync()
        {
            try
            {
                ServerAvailable = await Provider.IsAvailableAsync();
                if (ServerAvailable)
                {
                    Output.WriteLine("OpenAI API endpoint is available");
                }
                else
                {
                    Output.WriteLine("OpenAI API endpoint is not available");
                    throw new SkipException("OpenAI API endpoint is not available");
                }
            }
            catch (Exception ex) when (ex is not SkipException)
            {
                Output.WriteLine($"Failed to connect to OpenAI API endpoint: {ex.Message}");
                throw new SkipException("Failed to connect to OpenAI API endpoint");
            }
        }

        public Task DisposeAsync()
        {
            if (Provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            return Task.CompletedTask;
        }

        private class SkipException : Exception
        {
            public SkipException(string message) : base(message) { }
        }
    }
}