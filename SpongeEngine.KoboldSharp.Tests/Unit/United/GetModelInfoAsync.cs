using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.United
{
    public class GetModelInfoAsync : UnitTestBase
    {
        public GetModelInfoAsync(ITestOutputHelper output) : base(output) {}
        
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
    }
}