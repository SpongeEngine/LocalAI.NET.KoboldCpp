using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.United
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class GenerateAsync : IntegrationTestBase
    {
        public GenerateAsync(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GenerateAsync_WithSimplePrompt_ShouldReturnResponse()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Once upon a time",
                MaxLength = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40,
                RepetitionPenalty = 1.1f,
                RepetitionPenaltyRange = 64
            };

            // Act
            KoboldSharpClient.GenerateAsyncResponse response = await Client.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Results.Should().NotBeEmpty();
            response.Results[0].Text.Should().NotBeNullOrEmpty();
        }
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GenerateAsync_WithStopSequence_ShouldGenerateText()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Write a short story",
                MaxLength = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                StopSequences = new List<string> { "." }
            };

            // Act
            KoboldSharpClient.GenerateAsyncResponse response = await Client.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Results.Should().NotBeEmpty();
            response.Results[0].Text.Should().NotBeNullOrEmpty();
        }
        
        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GenerateAsync_WithDifferentTemperatures_ShouldWork()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Test various temperature settings
            float[] temperatures = new[] { 0.1f, 0.7f, 1.5f };
            foreach (float temp in temperatures)
            {
                // Arrange
                KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
                {
                    Prompt = "The quick brown fox",
                    MaxLength = 20,
                    Temperature = temp,
                    TopP = 0.9f,
                    TopK = 40,
                    RepetitionPenalty = 1.1f,
                    RepetitionPenaltyRange = 64
                };

                // Act
                KoboldSharpClient.GenerateAsyncResponse response = await Client.GenerateAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Results.Should().NotBeEmpty();
                response.Results[0].Text.Should().NotBeNullOrEmpty();
                Output.WriteLine($"Temperature {temp} response: {response.Results[0].Text}");
                
                // Those two temperature values produce unique enough outputs (often shorter or more abrupt) that cause the server’s final SSE chunk or the socket close to happen in a way .NET perceives as “premature.”
                // It’s a known type of SSE/chunked-encoding edge case—not actually a model crash or a timeout. It just tends to surface with certain generation lengths or timing patterns, which happen more at extremes like 0.1 and 1.5.
                await Task.Delay(500);
            }
        }
    }
}