using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Multiplayer
{
    [Trait("Category", "Integration")]
    [Trait("API", "Multiplayer")]
    public class MultiplayerStatusTests : IntegrationTestBase
    {
        public MultiplayerStatusTests(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        public async Task GetMultiplayerStatus_ShouldReturnValidStatus()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = "IntegrationTest",
                SenderBusy = false
            };

            // Act
            var result = await Client.GetMultiplayerStatusAsync(request);

            // Assert
            result.Should().NotBeNull();
            
            // Log status information
            Output.WriteLine($"Turn Major: {result.TurnMajor}");
            Output.WriteLine($"Turn Minor: {result.TurnMinor}");
            Output.WriteLine($"Is Idle: {result.IsIdle}");
            Output.WriteLine($"Data Format: {result.DataFormat}");
        }

        [SkippableFact]
        public async Task GetMultiplayerStatus_MultipleRequests_ShouldBeConsistent()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.MultiplayerStatusRequest
            {
                Sender = "IntegrationTest",
                SenderBusy = false
            };

            // Act - Get status multiple times
            var results = new List<KoboldSharpClient.MultiplayerStatusResponse>();
            for (int i = 0; i < 3; i++)
            {
                var result = await Client.GetMultiplayerStatusAsync(request);
                results.Add(result);
                
                Output.WriteLine($"\nRequest {i + 1}:");
                Output.WriteLine($"Turn Major: {result.TurnMajor}");
                Output.WriteLine($"Turn Minor: {result.TurnMinor}");
                Output.WriteLine($"Is Idle: {result.IsIdle}");
                
                await Task.Delay(1000);
            }

            // Assert - Check consistency
            results.Should().NotBeEmpty();
            var firstResult = results[0];
            results.Should().AllSatisfy(result =>
            {
                result.DataFormat.Should().Be(firstResult.DataFormat, 
                    "Data format should remain consistent");
            });
        }

        [SkippableFact]
        public async Task GetMultiplayerStatus_WithDifferentSenders_ShouldWork()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Test with different senders
            var senders = new[] { "Player1", "Player2", "Player3" };
            
            foreach (var sender in senders)
            {
                // Arrange
                var request = new KoboldSharpClient.MultiplayerStatusRequest
                {
                    Sender = sender,
                    SenderBusy = false
                };

                Output.WriteLine($"\nTesting with sender: {sender}");

                // Act
                var result = await Client.GetMultiplayerStatusAsync(request);

                // Assert
                result.Should().NotBeNull();
                Output.WriteLine($"Turn Major: {result.TurnMajor}");
                Output.WriteLine($"Turn Minor: {result.TurnMinor}");
                Output.WriteLine($"Is Idle: {result.IsIdle}");

                await Task.Delay(500);
            }
        }

        [SkippableFact]
        public async Task GetMultiplayerStatus_WithBusyState_ShouldReflectState()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var sender = "BusyTester";
            var states = new[] { false, true };

            foreach (var isBusy in states)
            {
                var request = new KoboldSharpClient.MultiplayerStatusRequest
                {
                    Sender = sender,
                    SenderBusy = isBusy
                };

                Output.WriteLine($"\nTesting with busy state: {isBusy}");

                // Act
                var result = await Client.GetMultiplayerStatusAsync(request);

                // Assert
                result.Should().NotBeNull();
                Output.WriteLine($"Is Idle: {result.IsIdle}");
                Output.WriteLine($"Turn Status: {result.TurnMajor}.{result.TurnMinor}");

                await Task.Delay(500);
            }
        }
    }
}