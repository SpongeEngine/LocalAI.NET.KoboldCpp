using LocalAI.NET.KoboldCpp.Providers.Native;
using LocalAI.NET.KoboldCpp.Tests.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace LocalAI.NET.KoboldCpp.Tests.Integration.Providers.Native
{
    public abstract class NativeTestBase : IAsyncLifetime
    {
        protected readonly INativeKoboldCppProvider Provider;
        protected readonly ITestOutputHelper Output;
        protected readonly ILogger Logger;
        protected bool ServerAvailable;

        protected NativeTestBase(ITestOutputHelper output)
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

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            Provider = new NativeKoboldCppProvider(httpClient, Logger, jsonSettings);
        }

        public async Task InitializeAsync()
        {
            try
            {
                ServerAvailable = await Provider.IsAvailableAsync();
                if (ServerAvailable)
                {
                    Output.WriteLine("KoboldCpp server is available");
                    var modelInfo = await Provider.GetModelInfoAsync();
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