﻿using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.StableDiffusion
{
    [Trait("Category", "Integration")]
    [Trait("API", "StableDiffusion")]
    public class InterrogateImage : IntegrationTestBase
    {
        public InterrogateImage(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        public async Task InterrogateImage_WithGeneratedImage_ShouldProvideCaption()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // First generate an image to interrogate
            var generateRequest = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A beautiful mountain landscape",
                Width = 512,
                Height = 512,
                Steps = 20
            };

            var generatedImage = await Client.TextToImageAsync(generateRequest);
            generatedImage.Images.Should().NotBeEmpty("Need a generated image to test interrogation");

            // Now interrogate the generated image
            var interrogateRequest = new KoboldSharpClient.InterrogateRequest
            {
                Image = generatedImage.Images[0]
            };

            // Act
            var result = await Client.InterrogateImageAsync(interrogateRequest);

            // Assert
            result.Should().NotBeNull();
            result.Caption.Should().NotBeNullOrEmpty();
            
            Output.WriteLine($"Original prompt: {generateRequest.Prompt}");
            Output.WriteLine($"Generated caption: {result.Caption}");
            
            // The caption should contain some relevant keywords
            result.Caption.ToLower().Should().ContainAny(
                new[] { "mountain", "landscape", "nature", "outdoor" },
                "Caption should be relevant to the generated image");
        }

        [SkippableFact]
        public async Task InterrogateImage_WithSampleImage_ShouldReturnCaption()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            var request = new KoboldSharpClient.InterrogateRequest
            {
                Image = SampleBase64Image
            };

            // Act
            var result = await Client.InterrogateImageAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Caption.Should().NotBeNullOrEmpty();
            Output.WriteLine($"Generated caption: {result.Caption}");
        }

        [SkippableFact]
        public async Task InterrogateImage_MultipleRequests_ShouldBeConsistent()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Generate an image for multiple interrogations
            var generateRequest = new KoboldSharpClient.StableDiffusionGenerationRequest
            {
                Prompt = "A simple red circle on white background",
                Width = 512,
                Height = 512,
                Steps = 20
            };

            var generatedImage = await Client.TextToImageAsync(generateRequest);
            generatedImage.Images.Should().NotBeEmpty("Need a generated image to test interrogation");

            // Perform multiple interrogations with retry logic
            var captions = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                // Add longer delay between requests to allow server to recover
                if (i > 0)
                {
                    Output.WriteLine($"Waiting before attempt {i + 1}...");
                    await Task.Delay(3000);
                }

                var interrogateRequest = new KoboldSharpClient.InterrogateRequest
                {
                    Image = generatedImage.Images[0]
                };

                // Add retry logic
                const int maxRetries = 3;
                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        var result = await Client.InterrogateImageAsync(interrogateRequest);
                        if (!string.IsNullOrEmpty(result.Caption))
                        {
                            captions.Add(result.Caption);
                            Output.WriteLine($"Attempt {i + 1} caption: {result.Caption}");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Output.WriteLine($"Attempt {i + 1} retry {retry + 1} failed: {ex.Message}");
                        if (retry == maxRetries - 1) throw;
                        await Task.Delay(2000 * (retry + 1)); // Exponential backoff
                    }
                }
            }

            captions.Should().NotBeEmpty("Should have obtained at least one caption");

            if (captions.Count > 1)
            {
                // Analyze common words only if we have multiple captions
                var commonWords = captions
                    .SelectMany(c => c.ToLower().Split(' '))
                    .GroupBy(w => w)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                Output.WriteLine($"Common words across captions: {string.Join(", ", commonWords)}");
                commonWords.Should().NotBeEmpty("Captions should share some common descriptive words");
            }
            else
            {
                Output.WriteLine("Only one caption was obtained, skipping common words analysis");
            }
        }

        [SkippableFact]
        public async Task InterrogateImage_WithDifferentImageSizes_ShouldWork()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            var sizes = new[] { (512, 512), (640, 384), (384, 640) };
            foreach (var (width, height) in sizes)
            {
                // Generate an image of specific size
                var generateRequest = new KoboldSharpClient.StableDiffusionGenerationRequest
                {
                    Prompt = "A simple test image",
                    Width = width,
                    Height = height,
                    Steps = 20
                };

                var generatedImage = await Client.TextToImageAsync(generateRequest);
                generatedImage.Images.Should().NotBeEmpty();

                // Interrogate the generated image
                var interrogateRequest = new KoboldSharpClient.InterrogateRequest
                {
                    Image = generatedImage.Images[0]
                };

                Output.WriteLine($"\nTesting {width}x{height} image:");
                var result = await Client.InterrogateImageAsync(interrogateRequest);
                
                result.Should().NotBeNull();
                result.Caption.Should().NotBeNullOrEmpty();
                Output.WriteLine($"Caption: {result.Caption}");

                await Task.Delay(1000);
            }
        }
    }
}