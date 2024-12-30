using LocalAI.NET.KoboldCpp.Providers.OpenAi;
using LocalAI.NET.KoboldCpp.Tests.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace LocalAI.NET.KoboldCpp.Tests.Integration.Providers.OpenAi
{
    public abstract class OpenAiTestBase : IAsyncLifetime
    {
        protected readonly IOpenAiKoboldCppProvider Provider;
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

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            Provider = new OpenAiKoboldCppProvider(
                httpClient, 
                modelName: "koboldcpp",
                logger: Logger, 
                jsonSettings: jsonSettings);
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