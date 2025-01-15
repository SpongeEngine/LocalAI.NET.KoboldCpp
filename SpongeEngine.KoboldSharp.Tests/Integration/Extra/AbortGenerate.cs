using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class AbortGenerate : IntegrationTestBase
    {
        public AbortGenerate(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        public async Task AbortGenerate_DuringGeneration_ShouldStop()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Start a long generation
            var request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Write a very long story about a journey",
                MaxLength = 200,
                Temperature = 0.7f,
                TopP = 0.9f
            };

            // Start generation in a separate task
            var generationTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var token in Client.GenerateStreamAsync(request))
                    {
                        Output.WriteLine($"Generated token: {token}");
                    }
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Generation stopped: {ex.Message}");
                }
            });

            // Give some time for generation to start
            await Task.Delay(1000);

            // Act
            var result = await Client.AbortGenerateAsync();

            // Assert
            result.Should().BeTrue();
            
            // Wait for generation task to complete due to abort
            var completedTask = await Task.WhenAny(generationTask, Task.Delay(5000));
            completedTask.Should().Be(generationTask, "Generation should stop after abort");
        }

        [SkippableFact]
        public async Task AbortGenerate_WhenNoGeneration_ShouldReturnTrue()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var result = await Client.AbortGenerateAsync();

            // Assert
            result.Should().BeTrue();
            Output.WriteLine("Successfully called abort with no active generation");
        }
    }
}