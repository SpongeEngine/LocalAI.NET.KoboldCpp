using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.United
{
    public class IsAvailableAsync : UnitTestBase
    {
        public IsAvailableAsync(ITestOutputHelper output) : base(output) {}
        
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
    }
}