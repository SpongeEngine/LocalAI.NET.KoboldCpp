using Microsoft.Extensions.Logging;
using SpongeEngine.KoboldSharp.Tests.Common;
using WireMock.Server;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit
{
    public abstract class UnitTestBase : TestBase
    {
        protected readonly WireMockServer Server;
        protected readonly KoboldSharpClient Client;

        protected UnitTestBase(ITestOutputHelper output)
        {
            Server = WireMockServer.Start();
            Client = new KoboldSharpClient(
                new KoboldSharpClientOptions() 
                {
                    HttpClient = new HttpClient
                    {
                        BaseAddress = new Uri(Server.Urls[0])
                    },
                    Logger = LoggerFactory
                        .Create(builder => builder.AddXUnit(output))
                        .CreateLogger(GetType()),
                });
        }
    }
}