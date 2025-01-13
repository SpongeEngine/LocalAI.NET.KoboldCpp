using Microsoft.Extensions.Logging;
using WireMock.Server;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit
{
    public abstract class UnitTestBase : IDisposable
    {
        protected readonly WireMockServer Server;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;

        protected UnitTestBase(ITestOutputHelper output)
        {
            Server = WireMockServer.Start();
            BaseUrl = Server.Urls[0];
            Logger = LoggerFactory
                .Create(builder => builder.AddXUnit(output))
                .CreateLogger(GetType());
        }

        public virtual void Dispose()
        {
            Server.Dispose();
        }
    }
}