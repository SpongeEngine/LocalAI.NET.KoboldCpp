using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.StableDiffusion
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class StableDiffusionModels : IntegrationTestBase
    {
        public StableDiffusionModels(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        public async Task GetStableDiffusionModels_ShouldReturnValidModels()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var models = await Client.GetStableDiffusionModelsAsync();

            // Assert
            models.Should().NotBeNull();
            
            // Log model information
            Output.WriteLine($"Found {models.Count} models:");
            foreach (var model in models)
            {
                Output.WriteLine($"\nModel: {model.Title}");
                Output.WriteLine($"Name: {model.ModelName}");
                Output.WriteLine($"Hash: {model.Hash}");
                Output.WriteLine($"Filename: {model.Filename}");
            }

            if (models.Any())
            {
                // If models are found, verify their properties
                models.Should().AllSatisfy(model =>
                {
                    model.Title.Should().NotBeNullOrEmpty();
                    model.ModelName.Should().NotBeNullOrEmpty();
                    model.Filename.Should().NotBeNullOrEmpty();
                });
            }
            else
            {
                Output.WriteLine("No Stable Diffusion models found - this may be normal if SD is not configured");
            }
        }

        [SkippableFact]
        public async Task GetStableDiffusionModels_ShouldHaveConsistentHashes()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Get models twice to verify consistency
            var firstResult = await Client.GetStableDiffusionModelsAsync();
            await Task.Delay(1000); // Brief delay between requests
            var secondResult = await Client.GetStableDiffusionModelsAsync();

            // Assert
            firstResult.Should().NotBeNull();
            secondResult.Should().NotBeNull();

            // Compare results
            firstResult.Count.Should().Be(secondResult.Count, "Model count should be consistent between requests");
            
            if (firstResult.Any())
            {
                // Compare model hashes
                for (int i = 0; i < firstResult.Count; i++)
                {
                    var model1 = firstResult[i];
                    var model2 = secondResult[i];

                    Output.WriteLine($"Comparing model: {model1.Title}");
                    model1.Hash.Should().Be(model2.Hash, 
                        $"Hash for model {model1.Title} should be consistent between requests");
                }
            }
        }
    }
}