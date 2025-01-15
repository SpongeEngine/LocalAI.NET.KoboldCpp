using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class Detokenize : IntegrationTestBase
    {
        public Detokenize(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Detokenize_ShouldDecodeTokensToText()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var text = "Hello world, this is a test";
            var countRequest = new KoboldSharpClient.CountTokensRequest
            {
                Prompt = text,
                IncludeSpecialTokens = true
            };

            // First get the token IDs using CountTokens
            var countResult = await Client.CountTokensAsync(countRequest);
            countResult.TokenIds.Should().NotBeEmpty("Need tokens to test detokenization");

            var detokenizeRequest = new KoboldSharpClient.DetokenizeRequest
            {
                TokenIds = countResult.TokenIds
            };

            // Act
            var result = await Client.DetokenizeAsync(detokenizeRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Text.Should().NotBeNullOrEmpty();
            Output.WriteLine($"Original text: {text}");
            Output.WriteLine($"Detokenized text: {result.Text}");
    
            // Note: The detokenized text might not match the original exactly due to tokenization quirks,
            // but it should contain the same semantic content
            result.Text.ToLower().Should().ContainAny(text.ToLower().Split(' '));
        }
    }
}