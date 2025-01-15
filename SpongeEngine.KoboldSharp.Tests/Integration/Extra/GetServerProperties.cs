using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class GetServerProperties : IntegrationTestBase
    {
        public GetServerProperties(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GetServerProperties_ShouldReturnValidConfiguration()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var result = await Client.GetServerPropertiesAsync();

            // Assert
            result.Should().NotBeNull();
            result.ChatTemplate.Should().NotBeNull();
            result.TotalSlots.Should().BeGreaterThan(0);
            result.Settings.Should().NotBeNull();
            result.Settings.ContextSize.Should().BeGreaterThan(0);
    
            Output.WriteLine($"Chat Template: {result.ChatTemplate}");
            Output.WriteLine($"Total Slots: {result.TotalSlots}");
            Output.WriteLine($"Context Size: {result.Settings.ContextSize}");
        }
    }
}