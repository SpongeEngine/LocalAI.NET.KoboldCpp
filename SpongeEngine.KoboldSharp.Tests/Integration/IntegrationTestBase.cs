using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Tests.Common;
using SpongeEngine.LLMSharp.Core.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration
{
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly KoboldSharpClient Client;
        protected readonly ITestOutputHelper Output;
        protected readonly ILogger Logger;
        protected bool ServerAvailable;

        protected IntegrationTestBase(ITestOutputHelper output)
        {
            Output = output;
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());

            var httpClient = new HttpClient 
            { 
                BaseAddress = new Uri(TestConfig.NativeApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(TestConfig.TimeoutSeconds)
            };

            Client = new KoboldSharpClient(httpClient, new LlmOptions(), "", TestConfig.BaseUrl, Logger);
        }

        public async Task InitializeAsync()
        {
            try
            {
                ServerAvailable = await Client.IsAvailableAsync();
                if (ServerAvailable)
                {
                    Output.WriteLine("KoboldCpp server is available");
                    var modelInfo = await Client.GetModelInfoAsync();
                    Output.WriteLine($"Connected to model: {modelInfo.ModelName}");
                }
                else
                {
                    Output.WriteLine("KoboldCpp server is not available");
                    throw new SkipException("KoboldCpp server is not available");
                }
            }
            catch (Exception ex) when (ex is not SkipException)
            {
                Output.WriteLine($"Failed to connect to KoboldCpp server: {ex.Message}");
                throw new SkipException("Failed to connect to KoboldCpp server");
            }
        }

        public Task DisposeAsync()
        {
            if (Client is IDisposable disposable)
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