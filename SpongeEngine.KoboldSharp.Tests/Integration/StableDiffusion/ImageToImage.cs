using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.StableDiffusion
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class ImageToImage : IntegrationTestBase
    {
        // Simple 1x1 pixel base64 encoded PNG
        private const string SampleBase64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";
        
        public ImageToImage(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        public async Task ImageToImage_WithBasicPrompt_ShouldGenerateImage()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { SampleBase64Image },
                Prompt = "Convert to oil painting style",
                DenoisingStrength = 0.75f,
                Steps = 20,
                CfgScale = 7.0f,
                SamplerName = "euler_a"
            };

            // Act
            var result = await Client.ImageToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty("At least one image should be generated");
            result.Parameters.Should().NotBeEmpty("Generation parameters should be returned");
            
            // Log generation details
            Output.WriteLine($"Generated {result.Images.Count} images");
            Output.WriteLine($"Parameters: {result.Info}");
            Output.WriteLine($"First image base64 length: {result.Images[0].Length}");
        }

        [SkippableFact]
        public async Task ImageToImage_WithDifferentDenoisingStrengths_ShouldWork()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

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

                Output.WriteLine($"\nTesting denoising strength: {strength}");

                // Act
                var result = await Client.ImageToImageAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Images.Should().NotBeEmpty();
                Output.WriteLine($"Successfully generated image with denoising strength {strength}");

                // Add delay between generations
                await Task.Delay(1000);
            }
        }

        [SkippableFact]
        public async Task ImageToImage_WithNegativePrompt_ShouldGenerateImage()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
            {
                InitImages = new List<string> { SampleBase64Image },
                Prompt = "Convert to watercolor painting",
                NegativePrompt = "blurry, distorted, low quality",
                DenoisingStrength = 0.7f,
                Steps = 20
            };

            // Act
            var result = await Client.ImageToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            
            Output.WriteLine($"Generated image with negative prompt");
            Output.WriteLine($"Parameters used: {result.Info}");
        }

        [SkippableFact]
        public async Task ImageToImage_WithVariousSteps_ShouldShowDifferentResults()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            var stepCounts = new[] { 10, 20, 30 };
            
            foreach (var steps in stepCounts)
            {
                // Arrange
                var request = new KoboldSharpClient.StableDiffusionImageToImageRequest
                {
                    InitImages = new List<string> { SampleBase64Image },
                    Prompt = "Convert to digital art",
                    DenoisingStrength = 0.7f,
                    Steps = steps
                };

                Output.WriteLine($"\nTesting with {steps} steps");

                // Act
                var result = await Client.ImageToImageAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Images.Should().NotBeEmpty();
                Output.WriteLine($"Successfully generated image with {steps} steps");
                Output.WriteLine($"Output image size: {result.Images[0].Length} bytes");

                // Add delay between generations
                await Task.Delay(1000);
            }
        }
    }
}