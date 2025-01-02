using FluentAssertions;
using SpongeEngine.KoboldSharp.Providers.KoboldSharpOpenAI;
using SpongeEngine.KoboldSharp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.Providers.KoboldSharpOpenAI
{
    public class KoboldSharpOpenAiProviderTests : TestBase
    {
        private readonly KoboldSharpOpenAiProvider _provider;
        private readonly HttpClient _httpClient;

        public KoboldSharpOpenAiProviderTests(ITestOutputHelper output) : base(output)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _provider = new KoboldSharpOpenAiProvider(_httpClient, logger: Logger);
        }

        [Fact]
        public async Task CompleteAsync_ShouldReturnValidResponse()
        {
            // Arrange
            const string expectedResponse = "Test response";
            Server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($"{{\"choices\": [{{\"text\": \"{expectedResponse}\"}}]}}"));

            // Act
            var response = await _provider.CompleteAsync("Test prompt");

            // Assert
            response.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task StreamCompletionAsync_ShouldStreamTokens()
        {
            // Arrange
            var tokens = new[] { "Hello", " world", "!" };
            var streamResponses = tokens.Select(token => 
                $"data: {{\"choices\": [{{\"text\": \"{token}\"}}]}}\n\n");

            Server
                .Given(Request.Create()
                    .WithPath("/v1/completions")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody(string.Join("", streamResponses))
                    .WithHeader("Content-Type", "text/event-stream"));

            // Act
            var receivedTokens = new List<string>();
            await foreach (var token in _provider.StreamCompletionAsync("Test prompt"))
            {
                receivedTokens.Add(token);
            }

            // Assert
            receivedTokens.Should().BeEquivalentTo(tokens);
        }

        [Fact]
        public async Task IsAvailableAsync_WhenServerResponds_ShouldReturnTrue()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/v1/models")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200));

            // Act
            var isAvailable = await _provider.IsAvailableAsync();

            // Assert
            isAvailable.Should().BeTrue();
        }

        public override void Dispose()
        {
            _httpClient.Dispose();
            base.Dispose();
        }
    }
}