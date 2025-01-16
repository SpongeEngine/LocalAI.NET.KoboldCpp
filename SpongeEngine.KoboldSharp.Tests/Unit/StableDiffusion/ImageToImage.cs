using FluentAssertions;
using Microsoft.Extensions.Logging;
using SpongeEngine.LLMSharp.Core.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Unit.StableDiffusion
{
    public class ImageToImage : UnitTestBase
    {
        public ImageToImage(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task ImageToImage_WithValidRequest_ShouldReturnModifiedImage()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { SampleBase64Image },
                Prompt = "Convert to oil painting",
                DenoisingStrength = 0.75f,
                Steps = 20,
                CfgScale = 7.0f,
                SamplerName = "euler_a"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody($@"{{
                        ""images"": [""{SampleBase64Image}""],
                        ""parameters"": {{
                            ""prompt"": ""Convert to oil painting"",
                            ""denoising_strength"": 0.75,
                            ""steps"": 20
                        }},
                        ""info"": ""Generation successful""
                    }}"));

            // Act
            var result = await Client.ImageToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            result.Images.Should().HaveCount(1);
            result.Images[0].Should().Be(SampleBase64Image);
            result.Parameters.Should().NotBeEmpty();
            result.Info.Should().Be("Generation successful");
        }

        [Fact]
        public async Task ImageToImage_WithDifferentDenoisingStrength_ShouldWork()
        {
            var strengths = new[] { 0.3f, 0.7f, 0.9f };
    
            foreach (var strength in strengths)
            {
                // Arrange
                var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
                {
                    InitImages = new List<string> { SampleBase64Image },
                    Prompt = "Convert to sketch",
                    DenoisingStrength = strength,
                    Steps = 20
                };

                // Mock successful response
                Server
                    .Given(Request.Create()
                        .WithPath("/sdapi/v1/img2img")
                        .WithBody(body => body.Contains($"\"denoising_strength\":{strength}"))
                        .UsingPost())
                    .RespondWith(Response.Create()
                        .WithStatusCode(200)
                        .WithBody(@"{
                    ""images"": [""base64_image_data""],
                    ""parameters"": {},
                    ""info"": """"
                }"));

                // Act
                var result = await Client.ImageToImageAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Images.Should().NotBeEmpty();
                Logger.LogInformation($"Successfully generated image with denoising strength {strength}");
            }
        }

        [Fact]
        public async Task ImageToImage_WithNoInitialImage_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string>(),
                Prompt = "Convert to oil painting"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("No input image provided"));

            // Act & Assert
            await Client.Invoking(c => c.ImageToImageAsync(request))
                .Should().ThrowAsync<LlmSharpException>()
                .WithMessage("Failed to generate image from image");
        }

        [Fact]
        public async Task ImageToImage_WithInvalidImage_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { "invalid_base64_data" },
                Prompt = "Convert to oil painting"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody("Invalid image data"));

            // Act & Assert
            await Client.Invoking(c => c.ImageToImageAsync(request))
                .Should().ThrowAsync<LlmSharpException>()
                .WithMessage("Failed to generate image from image");
        }

        [Fact]
        public async Task ImageToImage_WithServerError_ShouldThrowException()
        {
            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { SampleBase64Image },
                Prompt = "Convert to oil painting"
            };

            Server
                .Given(Request.Create()
                    .WithPath("/sdapi/v1/img2img")
                    .UsingPost())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Internal Server Error"));

            // Act & Assert
            await Client.Invoking(c => c.ImageToImageAsync(request))
                .Should().ThrowAsync<LlmSharpException>()
                .WithMessage("Failed to generate image from image");
        }
    }
}