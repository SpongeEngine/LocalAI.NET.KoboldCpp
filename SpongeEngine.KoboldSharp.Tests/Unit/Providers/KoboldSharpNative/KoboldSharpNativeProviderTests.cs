using FluentAssertions;
using SpongeEngine.KoboldSharp.Models;
using SpongeEngine.KoboldSharp.Providers.KoboldSharpNative;
using SpongeEngine.KoboldSharp.Tests.Common;
using SpongeEngine.LLMSharp.Core.Configuration;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Providers.KoboldSharpNative
{
    public class KoboldSharpNativeProviderTests : TestBase
    {
        private readonly KoboldSharpNativeProvider _provider;
        private readonly HttpClient _httpClient;

        public KoboldSharpNativeProviderTests(ITestOutputHelper output) : base(output)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _provider = new KoboldSharpNativeProvider(_httpClient, new LlmOptions(), "KoboldCpp", TestConfig.BaseUrl, Logger);
        }

        [Fact]
        public async Task GenerateAsync_WithValidRequest_ShouldReturnResponse()
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
            var response = await _provider.GenerateAsync(request);

            // Assert
            response.Results.Should().ContainSingle()
                .Which.Text.Should().Be(expectedResponse);
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

            var request = new KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80,
                Stream = true
            };

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _provider.GenerateStreamAsync(request))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        public override void Dispose()
        {
            _httpClient.Dispose();
            base.Dispose();
        }
    }
}