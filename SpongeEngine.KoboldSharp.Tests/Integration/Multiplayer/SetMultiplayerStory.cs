using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Multiplayer
{
    [Trait("Category", "Integration")]
    [Trait("API", "Multiplayer")]
    public class SetMultiplayerStoryTests : IntegrationTestBase
    {
        public SetMultiplayerStoryTests(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        public async Task SetMultiplayerStory_WithNewContent_ShouldUpdateStory()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            string testContent = $"Test story created at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            var request = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = testContent,
                Sender = "IntegrationTest",
                DataFormat = "plain"
            };

            // Act
            var result = await Client.SetMultiplayerStoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue("Story update should succeed");
            
            // Verify content was updated
            var updatedStory = await Client.GetMultiplayerStoryAsync();
            updatedStory.Should().Contain(testContent);

            // Log results
            Output.WriteLine($"Set story result:");
            Output.WriteLine($"Success: {result.Success}");
            Output.WriteLine($"Turn Major: {result.TurnMajor}");
            Output.WriteLine($"Turn Minor: {result.TurnMinor}");
            Output.WriteLine($"Is Idle: {result.IsIdle}");
        }

        [SkippableFact]
        public async Task SetMultiplayerStory_WithIncrementalUpdates_ShouldAppendContent()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Start with a clean story
            var initialRequest = new KoboldSharpClient.MultiplayerStoryRequest
            {
                FullUpdate = true,
                Data = "Initial story content.\n",
                Sender = "IntegrationTest",
                DataFormat = "plain"
            };

            await Client.SetMultiplayerStoryAsync(initialRequest);
            await Task.Delay(1000); // Give time for update to process

            // Act - Add incremental updates
            var updates = new[]
            {
                "First update: The story continues...\n",
                "Second update: More content added...\n",
                "Final update: The conclusion.\n"
            };

            foreach (var update in updates)
            {
                var updateRequest = new KoboldSharpClient.MultiplayerStoryRequest
                {
                    FullUpdate = false,
                    Data = update,
                    Sender = "IntegrationTest",
                    DataFormat = "plain"
                };

                var result = await Client.SetMultiplayerStoryAsync(updateRequest);
                result.Success.Should().BeTrue();
                Output.WriteLine($"Added update: {update.TrimEnd()}");
                
                await Task.Delay(1000); // Give time between updates
            }

            // Verify final content
            var finalStory = await Client.GetMultiplayerStoryAsync();
            finalStory.Should().Contain("Initial story content");
            foreach (var update in updates)
            {
                finalStory.Should().Contain(update.TrimEnd());
            }

            Output.WriteLine("\nFinal story content:");
            Output.WriteLine(finalStory);
        }

        [SkippableFact]
        public async Task SetMultiplayerStory_WithDifferentFormats_ShouldHandleFormatting()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Test different data formats
            var formats = new[]
            {
                ("plain", "Simple plain text content"),
                ("json", "{\"content\": \"JSON formatted content\"}"),
                ("html", "<p>HTML formatted content</p>")
            };

            foreach (var (format, content) in formats)
            {
                // Arrange
                var request = new KoboldSharpClient.MultiplayerStoryRequest
                {
                    FullUpdate = true,
                    Data = content,
                    Sender = "IntegrationTest",
                    DataFormat = format
                };

                Output.WriteLine($"\nTesting format: {format}");

                // Act
                var result = await Client.SetMultiplayerStoryAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Success.Should().BeTrue();
                result.DataFormat.Should().Be(format);
                
                Output.WriteLine($"Update successful with format {format}");
                await Task.Delay(1000);
            }
        }
        
        private class UserResult
        {
            public string Username { get; }
            public bool WasSuccessful { get; }
            public string? ErrorMessage { get; }

            public UserResult(string username, bool wasSuccessful, string? errorMessage)
            {
                Username = username;
                WasSuccessful = wasSuccessful;
                ErrorMessage = errorMessage;
            }
        }

        [SkippableFact]
        public async Task SetMultiplayerStory_WithConcurrentUsers_ShouldHandleRaceConditions()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange - Multiple users trying to update simultaneously
            var users = new[] { "User1", "User2", "User3" };
            
            var tasks = users.Select(async user =>
            {
                var request = new KoboldSharpClient.MultiplayerStoryRequest
                {
                    FullUpdate = true,
                    Data = $"Content from {user}",
                    Sender = user,
                    DataFormat = "plain"
                };

                try
                {
                    var result = await Client.SetMultiplayerStoryAsync(request);
                    Output.WriteLine($"Result for {user}: Success={result.Success}");
                    return new UserResult(user, result.Success, result.Error);
                }
                catch (Exception ex)
                {
                    Output.WriteLine($"Error for {user}: {ex.Message}");
                    return new UserResult(user, false, ex.Message);
                }
            }).ToList();

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            results.Should().NotBeNull();
            
            // Log results
            foreach (var result in results)
            {
                Output.WriteLine($"\nUser: {result.Username}");
                Output.WriteLine($"Success: {result.WasSuccessful}");
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Output.WriteLine($"Error: {result.ErrorMessage}");
                }
            }

            // At least one user should succeed
            results.Count(r => r.WasSuccessful).Should().BeGreaterThan(0);
        }
    }
}