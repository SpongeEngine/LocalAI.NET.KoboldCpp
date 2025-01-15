using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.United
{
    public class GenerateAsync : UnitTestBase
    {
        public GenerateAsync(ITestOutputHelper output) : base(output) {}
        
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

            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80
            };

            // Act
            var response = await Client.GenerateAsync(request);

            // Assert
            response.Results.Should().ContainSingle().Which.Text.Should().Be(expectedResponse);
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

            KoboldSharpClient.KoboldSharpRequest request = new KoboldSharpClient.KoboldSharpRequest
            {
                Prompt = "Test prompt",
                MaxLength = 80
            };

            // Act
            var response = await Client.GenerateAsync(request);

            // Assert
            response.Results.Should().ContainSingle()
                .Which.Text.Should().Be(expectedResponse);
        }
    }
}