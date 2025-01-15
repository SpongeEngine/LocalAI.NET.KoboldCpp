using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.KoboldSharp.Tests.Common;
using WireMock.Server;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit
{
    public abstract class UnitTestBase : IDisposable
    {
        protected readonly WireMockServer Server;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;
        protected readonly KoboldSharpClient Client;
        protected readonly HttpClient HttpClient;

        protected UnitTestBase(ITestOutputHelper output)
        {
            Server = WireMockServer.Start();
            BaseUrl = Server.Urls[0];
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());
            HttpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            Client = new KoboldSharpClient(
                new KoboldSharpClientOptions() 
                {
                    HttpClient = HttpClient,
                    BaseUrl = TestConfig.BaseUrl,
                    Logger = Logger,
                });
        }

        public virtual void Dispose()
        {
            HttpClient.Dispose();
            Server.Dispose();
        }
    }
}