using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class LogProbs : IntegrationTestBase
    {
        public LogProbs(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        public async Task GetPendingOutput_DuringGeneration_ShouldReturnPartialOutput()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Start a long generation
            var request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Write a long story about an adventure",
                MaxLength = 100,
                Temperature = 0.7f,
                TopP = 0.9f
            };

            // Start generation in background
            var generationTask = Task.Run(async () =>
            {
                await Client.GenerateAsync(request);
            });

            // Give some time for generation to start
            await Task.Delay(500);

            // Act
            var pendingOutput = await Client.GetPendingOutputAsync();

            // Assert
            pendingOutput.Should().NotBeNull();
            if (!string.IsNullOrEmpty(pendingOutput))
            {
                Output.WriteLine($"Pending output received: {pendingOutput}");
            }
            else
            {
                Output.WriteLine("No pending output at the moment of check");
            }

            // Cleanup - ensure generation completes or is aborted
            if (!generationTask.IsCompleted)
            {
                await Client.AbortGenerateAsync();
            }
            await Task.WhenAny(generationTask, Task.Delay(5000));
        }

        [SkippableFact]
        public async Task GetPendingOutput_WithNoGeneration_ShouldReturnEmpty()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // First ensure no generation is running
            await Client.AbortGenerateAsync();
            await Task.Delay(500); // Give time for any pending operations to complete

            // Act
            var result = await Client.GetPendingOutputAsync();

            // Assert
            result.Should().BeEmpty();
            Output.WriteLine("Successfully verified no pending output when no generation is active");
        }

        [SkippableFact]
        public async Task GetPendingOutput_MultipleChecks_ShouldShowProgress()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Start a long generation
            var request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Write a detailed story about a journey through time",
                MaxLength = 150,
                Temperature = 0.7f,
                TopP = 0.9f
            };

            var generationTask = Task.Run(async () =>
            {
                await Client.GenerateAsync(request);
            });

            // Act - Check output multiple times
            var outputs = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(1000); // Wait between checks
                var pendingOutput = await Client.GetPendingOutputAsync();
                outputs.Add(pendingOutput);
                Output.WriteLine($"Check {i + 1} output length: {pendingOutput.Length}");
                Output.WriteLine($"Content: {pendingOutput}");
            }

            // Assert
            outputs.Should().NotBeEmpty();
            if (outputs.Count > 1)
            {
                // Verify that at least some outputs show different content
                outputs.Distinct().Should().HaveCountGreaterThan(1, 
                    "Multiple checks should show generation progress");
            }

            // Cleanup
            if (!generationTask.IsCompleted)
            {
                await Client.AbortGenerateAsync();
            }
            await Task.WhenAny(generationTask, Task.Delay(5000));
        }
    }
}