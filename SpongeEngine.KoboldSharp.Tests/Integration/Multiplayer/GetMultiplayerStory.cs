using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Multiplayer
{
    [Trait("Category", "Integration")]
    [Trait("API", "Multiplayer")]
    public class GetMultiplayerStoryTests : IntegrationTestBase
    {
        public GetMultiplayerStoryTests(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        public async Task GetMultiplayerStory_ShouldRetrieveStory()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var result = await Client.GetMultiplayerStoryAsync();

            // Assert
            result.Should().NotBeNull();
            
            // Log the result
            if (string.IsNullOrEmpty(result))
            {
                Output.WriteLine("No active story found");
            }
            else
            {
                Output.WriteLine($"Retrieved story of length: {result.Length}");
                Output.WriteLine($"First 100 characters: {result.Substring(0, Math.Min(100, result.Length))}");
            }
        }

        [SkippableFact]
        public async Task GetMultiplayerStory_MultipleRequests_ShouldBeConsistent()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act - Get story multiple times
            var firstStory = await Client.GetMultiplayerStoryAsync();
            await Task.Delay(1000);
            var secondStory = await Client.GetMultiplayerStoryAsync();
            await Task.Delay(1000);
            var thirdStory = await Client.GetMultiplayerStoryAsync();

            // Assert
            firstStory.Should().NotBeNull();
            secondStory.Should().NotBeNull();
            thirdStory.Should().NotBeNull();

            // Log results
            Output.WriteLine($"First request length: {firstStory.Length}");
            Output.WriteLine($"Second request length: {secondStory.Length}");
            Output.WriteLine($"Third request length: {thirdStory.Length}");

            // Check consistency unless story was actively being modified
            if (firstStory.Length == secondStory.Length && secondStory.Length == thirdStory.Length)
            {
                firstStory.Should().Be(secondStory);
                secondStory.Should().Be(thirdStory);
                Output.WriteLine("All retrieved stories were identical");
            }
            else
            {
                Output.WriteLine("Story length changed between requests - may be actively modified");
            }
        }

        [SkippableFact]
        public async Task GetMultiplayerStory_AfterSetStory_ShouldReflectChanges()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Set a new story
            var testStory = $"Test story created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            var setRequest = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = testStory,
                Sender = "IntegrationTest",
                DataFormat = "plain"
            };

            await Client.SetMultiplayerStoryAsync(setRequest);
            await Task.Delay(1000); // Give time for the change to take effect

            // Act
            var retrievedStory = await Client.GetMultiplayerStoryAsync();

            // Assert
            retrievedStory.Should().NotBeNull();
            retrievedStory.Should().Contain(testStory);
            
            Output.WriteLine("Original story:");
            Output.WriteLine(testStory);
            Output.WriteLine("\nRetrieved story:");
            Output.WriteLine(retrievedStory);
        }

        [SkippableFact]
        public async Task GetMultiplayerStory_WithActiveUsers_ShouldHandleConcurrency()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Simulate multiple users reading the story concurrently
            var tasks = new List<Task<string>>();
            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Client.GetMultiplayerStoryAsync());
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().NotBeNull();
            results.Should().AllSatisfy(story => story.Should().NotBeNull());

            // Log results
            for (int i = 0; i < results.Length; i++)
            {
                Output.WriteLine($"Request {i + 1} length: {results[i].Length}");
            }

            // Check if all requests got the same story
            var allSame = results.All(r => r == results[0]);
            Output.WriteLine($"All concurrent requests returned identical stories: {allSame}");
        }
    }
}