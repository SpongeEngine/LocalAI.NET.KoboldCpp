using FluentAssertions;
using LocalAI.NET.KoboldCpp.Providers.OpenAi;
using LocalAI.NET.KoboldCpp.Tests.Common;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace LocalAI.NET.KoboldCpp.Tests.Unit.Providers.OpenAi
{
    public class OpenAiKoboldCppProviderTests : KoboldCppTestBase
    {
        private readonly OpenAiKoboldCppProvider _provider;
        private readonly HttpClient _httpClient;

        public OpenAiKoboldCppProviderTests(ITestOutputHelper output) : base(output)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            _provider = new OpenAiKoboldCppProvider(_httpClient, logger: Logger);
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