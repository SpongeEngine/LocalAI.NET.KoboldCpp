using FluentAssertions;
using SpongeEngine.KoboldSharp.Client;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.KoboldSharp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Client
{
    public class KoboldSharpClientTests : TestBase
    {
        private readonly KoboldSharpClient _client;

        public KoboldSharpClientTests(ITestOutputHelper output) : base(output)
        {
            _client = new KoboldSharpClient(new KoboldSharpOptions
            {
                BaseUrl = BaseUrl
            }, Logger);
        }

        [Fact]
        public async Task GenerateAsync_ShouldReturnValidResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/generate")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"results\": [{{\"text\": \"{expectedResponse}\", \"tokens\": 3}}]}}"));

            var request = new KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80
            };

            // Act
            var response = await _client.GenerateAsync(request);

            // Assert
            response.Results.Should().ContainSingle()
                .Which.Text.Should().Be(expectedResponse);
        }

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

            var request = new KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80,
                Stream = true
            };

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _client.GenerateStreamAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task GetModelInfoAsync_ShouldReturnModelInfo()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/model")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"result\": \"test-model\"}"));
            
            // Act
            var modelInfo = await _client.GetModelInfoAsync();

            // Assert
            modelInfo.ModelName.Should().Be("test-model");
        }

        [Fact]
        public async Task IsAvailableAsync_WhenServerResponds_ShouldReturnTrue()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/model")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            // Act
            var isAvailable = await _client.IsAvailableAsync();

            // Assert
            isAvailable.Should().BeTrue();
        }
    }
}