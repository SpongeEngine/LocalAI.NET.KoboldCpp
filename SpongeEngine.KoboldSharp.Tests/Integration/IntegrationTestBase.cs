using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration
{
    public abstract class IntegrationTestBase : TestBase, IAsyncLifetime
    {
        protected readonly KoboldSharpClient? Client;
        protected readonly ITestOutputHelper Output;
        protected bool ServerAvailable;

        protected IntegrationTestBase(ITestOutputHelper output)
        {
            Output = output;
            
            Client = new KoboldSharpClient(
                new KoboldSharpClientOptions() 
                {
                    HttpClient = new HttpClient 
                    { 
                        BaseAddress = new Uri(TestConfig.BaseUrl)
                    },
                    Logger = LoggerFactory
                        .Create(builder => builder.AddXUnit(output))
                        .CreateLogger(GetType())
                });
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

        public async Task DisposeAsync()
        {
            Client?.Options.HttpClient?.Dispose();
            
            await Task.CompletedTask;
        }
    }
}