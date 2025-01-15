using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class CountTokens : IntegrationTestBase
    {
        public CountTokens(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task CountTokens_ShouldReturnValidCount()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.CountTokensRequest
            {
                Prompt = "Hello world, this is a test prompt",
                IncludeSpecialTokens = true
            };

            // Act
            var result = await Client.CountTokensAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Count.Should().BeGreaterThan(0);
            result.TokenIds.Should().NotBeEmpty();
            Output.WriteLine($"Token count: {result.Count}");
            Output.WriteLine($"Token IDs: {string.Join(", ", result.TokenIds)}");
        }
    }
}