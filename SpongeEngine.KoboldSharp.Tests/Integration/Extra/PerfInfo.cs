using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Extra
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class PerfInfo : IntegrationTestBase
    {
        public PerfInfo(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        public async Task GetPerfInfo_ShouldReturnValidData()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var result = await Client.GetPerfInfoAsync();

            // Assert
            result.Should().NotBeNull();
            result.TotalGenerations.Should().BeGreaterOrEqualTo(0);
            result.Uptime.Should().BeGreaterThan(0);
            
            // Log performance data
            Output.WriteLine($"Last Process Time: {result.LastProcessTime}s");
            Output.WriteLine($"Last Eval Time: {result.LastEvalTime}s");
            Output.WriteLine($"Last Token Count: {result.LastTokenCount}");
            Output.WriteLine($"Total Generations: {result.TotalGenerations}");
            Output.WriteLine($"Queue Size: {result.QueueSize}");
            Output.WriteLine($"Uptime: {result.Uptime}s");
            Output.WriteLine($"Idle Time: {result.IdleTime}s");
        }

        [SkippableFact]
        public async Task GetPerfInfo_AfterGeneration_ShouldShowActivity()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Get initial performance info
            var initialInfo = await Client.GetPerfInfoAsync();

            // Perform a generation
            var request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Test generation for performance monitoring",
                MaxLength = 20
            };
            await Client.GenerateAsync(request);

            // Act - Get updated performance info
            var updatedInfo = await Client.GetPerfInfoAsync();

            // Assert
            updatedInfo.Should().NotBeNull();
            updatedInfo.TotalGenerations.Should().BeGreaterThan(initialInfo.TotalGenerations);
            updatedInfo.LastTokenCount.Should().BeGreaterThan(0);
            updatedInfo.LastProcessTime.Should().BeGreaterThan(0);

            Output.WriteLine($"Initial Total Generations: {initialInfo.TotalGenerations}");
            Output.WriteLine($"Updated Total Generations: {updatedInfo.TotalGenerations}");
            Output.WriteLine($"Last Process Time: {updatedInfo.LastProcessTime}s");
            Output.WriteLine($"Last Token Count: {updatedInfo.LastTokenCount}");
        }
    }
}