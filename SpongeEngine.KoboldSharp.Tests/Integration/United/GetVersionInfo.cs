using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.United
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class GetVersionInfo : IntegrationTestBase
    {
        public GetVersionInfo(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GetVersionInfo_ShouldReturnValidVersion()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var versionInfo = await Client.GetVersionInfoAsync();

            // Assert
            versionInfo.Should().NotBeNull();
            versionInfo.Version.Should().NotBeNullOrEmpty();
            Output.WriteLine($"Server version: {versionInfo.Version}");
        }
    }
}