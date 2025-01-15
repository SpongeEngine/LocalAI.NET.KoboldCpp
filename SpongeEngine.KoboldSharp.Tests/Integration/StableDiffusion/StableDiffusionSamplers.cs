using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.StableDiffusion
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class StableDiffusionSamplers : IntegrationTestBase
    {
        public StableDiffusionSamplers(ITestOutputHelper output) : base(output) {}
        
        [SkippableFact]
        public async Task GetStableDiffusionSamplers_ShouldReturnValidSamplers()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var samplers = await Client.GetStableDiffusionSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();
            
            // Log sampler information
            Output.WriteLine($"Found {samplers.Count} samplers:");
            foreach (var sampler in samplers)
            {
                Output.WriteLine($"\nSampler: {sampler.Name}");
                Output.WriteLine($"Aliases: {string.Join(", ", sampler.Aliases)}");
                Output.WriteLine("Options:");
                foreach (var (key, value) in sampler.Options)
                {
                    Output.WriteLine($"  {key}: {value}");
                }
            }

            if (samplers.Any())
            {
                // Verify basic sampler properties
                samplers.Should().AllSatisfy(sampler =>
                {
                    sampler.Name.Should().NotBeNullOrEmpty();
                    sampler.Aliases.Should().NotBeNull();
                    sampler.Options.Should().NotBeNull();
                });

                // Common samplers that should typically be available
                samplers.Should().Contain(s => 
                    s.Name.Contains("Euler", StringComparison.OrdinalIgnoreCase) ||
                    s.Aliases.Any(a => a.Contains("euler", StringComparison.OrdinalIgnoreCase)),
                    "Should contain at least one Euler-based sampler");
            }
            else
            {
                Output.WriteLine("No Stable Diffusion samplers found - this may be normal if SD is not configured");
            }
        }

        [SkippableFact]
        public async Task GetStableDiffusionSamplers_ShouldHaveConsistentOptions()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Get samplers twice to verify consistency
            var firstResult = await Client.GetStableDiffusionSamplersAsync();
            await Task.Delay(1000); // Brief delay between requests
            var secondResult = await Client.GetStableDiffusionSamplersAsync();

            // Assert
            firstResult.Should().NotBeNull();
            secondResult.Should().NotBeNull();

            // Compare results
            firstResult.Count.Should().Be(secondResult.Count, "Sampler count should be consistent between requests");
            
            if (firstResult.Any())
            {
                // Compare each sampler's properties
                for (int i = 0; i < firstResult.Count; i++)
                {
                    var sampler1 = firstResult[i];
                    var sampler2 = secondResult[i];

                    Output.WriteLine($"Comparing sampler: {sampler1.Name}");
                    sampler1.Name.Should().Be(sampler2.Name);
                    sampler1.Aliases.Should().BeEquivalentTo(sampler2.Aliases);
                    sampler1.Options.Should().BeEquivalentTo(sampler2.Options);
                }
            }
        }

        [SkippableFact]
        public async Task GetStableDiffusionSamplers_OptionsShouldBeValid()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Act
            var samplers = await Client.GetStableDiffusionSamplersAsync();

            // Assert
            samplers.Should().NotBeNull();

            foreach (var sampler in samplers)
            {
                Output.WriteLine($"\nValidating sampler: {sampler.Name}");
                
                // Verify options contain expected keys
                if (sampler.Options.Any())
                {
                    sampler.Options.Should().ContainKey("steps", 
                        "Sampler should have 'steps' configuration");

                    // Log all options for inspection
                    foreach (var (key, value) in sampler.Options)
                    {
                        Output.WriteLine($"Option {key}: {value}");
                        value.Should().NotBeNull("Option values should not be null");
                    }
                }
                else
                {
                    Output.WriteLine("No options found for this sampler");
                }

                // Verify aliases are properly formatted
                foreach (var alias in sampler.Aliases)
                {
                    alias.Should().NotBeNullOrWhiteSpace("Aliases should be valid strings");
                }
            }
        }
    }
}