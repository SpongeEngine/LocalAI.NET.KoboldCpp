using Microsoft.Extensions.Logging;
using WireMock.Server;
using Xunit.Abstractions;

namespace LocalAI.NET.KoboldCpp.Tests.Common
{
    public abstract class KoboldCppTestBase : IDisposable
    {
        protected readonly WireMockServer Server;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;

        protected KoboldCppTestBase(ITestOutputHelper output)
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