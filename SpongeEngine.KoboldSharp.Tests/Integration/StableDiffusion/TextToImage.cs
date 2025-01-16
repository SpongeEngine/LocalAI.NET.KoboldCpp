using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.StableDiffusion
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class TextToImage : IntegrationTestBase
    {
        public TextToImage(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        public async Task TextToImage_WithBasicPrompt_ShouldGenerateImage()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A simple landscape with mountains",
                Width = 512,
                Height = 512,
                Steps = 20,
                CfgScale = 7.0f,
                SamplerName = "euler_a"
            };

            // Act
            var result = await Client.TextToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty("At least one image should be generated");
    
            // Log generation details
            Output.WriteLine($"Generated {result.Images.Count} images");
            Output.WriteLine($"Parameters: {result.Info}");
            Output.WriteLine($"First image base64 length: {result.Images[0].Length}");
        }

        [SkippableFact]
        public async Task TextToImage_WithDifferentSamplers_ShouldWork()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Get available samplers
            var samplers = await Client.GetStableDiffusionSamplersAsync();
            Skip.If(!samplers.Any(), "No Stable Diffusion samplers available");

            // Test first two samplers
            var samplersToTest = samplers.Take(2).ToList();
            foreach (var sampler in samplersToTest)
            {
                // Arrange
                var request = new KoboldSharpClient.StableDiffusionGenerationRequest
                {
                    Prompt = "A simple test image",
                    Width = 512,
                    Height = 512,
                    Steps = 20,
                    SamplerName = sampler.Name
                };

                Output.WriteLine($"\nTesting sampler: {sampler.Name}");

                // Act
                var result = await Client.TextToImageAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Images.Should().NotBeEmpty();
                Output.WriteLine($"Successfully generated image with sampler {sampler.Name}");

                // Add delay between generations
                await Task.Delay(1000);
            }
        }

        [SkippableFact]
        public async Task TextToImage_WithNegativePrompt_ShouldGenerateImage()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A beautiful mountain landscape with clear sky",
                NegativePrompt = "clouds, blur, darkness, distortion",
                Width = 512,
                Height = 512,
                Steps = 20,
                CfgScale = 7.5f
            };

            // Act
            var result = await Client.TextToImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Images.Should().NotBeEmpty();
            
            Output.WriteLine($"Generated image with negative prompt");
            Output.WriteLine($"Parameters used: {result.Info}");
        }

        [SkippableFact]
        public async Task TextToImage_WithDifferentSizes_ShouldGenerateImage()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            var sizes = new[] { (512, 512), (640, 384), (384, 640) };
            
            foreach (var (width, height) in sizes)
            {
                // Arrange
                var request = new KoboldSharpClient.StableDiffusionGenerationRequest
                {
                    Prompt = "A simple landscape",
                    Width = width,
                    Height = height,
                    Steps = 20
                };

                Output.WriteLine($"\nTesting size: {width}x{height}");

                // Act
                var result = await Client.TextToImageAsync(request);

                // Assert
                result.Should().NotBeNull();
                result.Images.Should().NotBeEmpty();
                Output.WriteLine($"Successfully generated {width}x{height} image");

                // Add delay between generations
                await Task.Delay(1000);
            }
        }
    }
}