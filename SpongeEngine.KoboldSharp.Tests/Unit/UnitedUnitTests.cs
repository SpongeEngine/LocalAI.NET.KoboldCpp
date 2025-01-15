using FluentAssertions;
using SpongeEngine.LLMSharp.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit
{
    public class UnitedUnitTests : UnitTestBase
    {
        public UnitedUnitTests(ITestOutputHelper output) : base(output) {}
        
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
            var modelInfo = await Client.GetModelInfoAsync();

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
            var isAvailable = await Client.IsAvailableAsync();

            // Assert
            isAvailable.Should().BeTrue();
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
        
        [Fact]
        public async Task GetVersionInfoAsync_ShouldReturnVersionInfo()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/info/version")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("{\"result\": \"1.2.3\"}"));
    
            // Act
            var versionInfo = await Client.GetVersionInfoAsync();

            // Assert
            versionInfo.Should().NotBeNull();
            versionInfo.Version.Should().Be("1.2.3");
        }
        
        [Fact]
        public async Task GetVersionInfoAsync_WhenServerError_ShouldThrowException()
        {
            // Arrange
            Server
                .Given(Request.Create()
                    .WithPath("/api/v1/info/version")
                    .UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));
    
            // Act & Assert
            await Client.Invoking(c => c.GetVersionInfoAsync())
                .Should().ThrowAsync<LlmSharpException>()
                .WithMessage("Failed to get version info");
        }
    }
}