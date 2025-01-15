using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class UnitedIntegrationTests : IntegrationTestBase
    {
        public UnitedIntegrationTests(ITestOutputHelper output) : base(output) {}
    }
}