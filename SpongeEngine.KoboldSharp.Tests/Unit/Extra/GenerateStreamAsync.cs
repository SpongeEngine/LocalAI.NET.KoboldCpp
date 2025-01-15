using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Extra
{
    public class GenerateStreamAsync : UnitTestBase
    {
        public GenerateStreamAsync(ITestOutputHelper output) : base(output) {}
        
        [Fact]
        public async Task GenerateStreamAsync_ShouldStreamTokens()
        {
            // Arrange
            var tokens = new[] { "Hello", " world", "!" };
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/stream")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("data: {\"token\": \"Hello\", \"complete\": false}\n\n" +
                              "data: {\"token\": \" world\", \"complete\": false}\n\n" +
                              "data: {\"token\": \"!\", \"complete\": true}\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80,
                Stream = true
            };

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in Client.GenerateStreamAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }
        
        [Fact]
        public async Task GenerateStreamAsync_ShouldStreamResponse()
        {
            // Arrange
            var tokens = new[] { "Hello", " world", "!" };
            Server
                .Given(Request.Create()
                    .WithPath("/api/extra/generate/stream")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("data: {\"token\": \"Hello\", \"complete\": false}\n\n" +
                              "data: {\"token\": \" world\", \"complete\": false}\n\n" +
                              "data: {\"token\": \"!\", \"complete\": true}\n\n")
                    .WithHeader("Content-Type", "text/event-stream"));

            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80,
                Stream = true
            };

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in Client.GenerateStreamAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }
    }
}