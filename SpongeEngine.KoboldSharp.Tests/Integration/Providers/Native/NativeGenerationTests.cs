using FluentAssertions;
using SpongeEngine.KoboldSharp.Models;
using Xunit;
using Xunit.Abstractions;

namespace SpongeEngine.KoboldSharp.Tests.Integration.Providers.Native
{
    [Trait("Category", "Integration")]
    [Trait("API", "Native")]
    public class NativeGenerationTests : NativeTestBase
    {
        public NativeGenerationTests(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Generate_WithSimplePrompt_ShouldReturnResponse()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            KoboldSharpRequest request = new KoboldSharpRequest
            {
                Prompt = "Once upon a time",
                MaxLength = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40,
                RepetitionPenalty = 1.1f,
                RepetitionPenaltyRange = 64
            };

            // Act
            KoboldSharpResponse response = await Provider.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Results.Should().NotBeEmpty();
            response.Results[0].Text.Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task GenerateStream_ShouldStreamTokens()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            KoboldSharpRequest request = new KoboldSharpRequest
            {
                Prompt = "Write a short story about",
                MaxLength = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                TopK = 40,
                RepetitionPenalty = 1.1f,
                RepetitionPenaltyRange = 64,
                TrimStop = true
            };

            List<string> tokens = new List<string>();
            using CancellationTokenSource? cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                await foreach (String token in Provider.GenerateStreamAsync(request, cts.Token))
                {
                    tokens.Add(token);
                    Output.WriteLine($"Received token: {token}");

                    if (tokens.Count >= request.MaxLength)
                    {
                        Output.WriteLine("Reached max length, breaking");
                        break;
                    }
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                Output.WriteLine($"Stream timed out after receiving {tokens.Count} tokens");
            }
            catch (Exception ex)
            {
                Output.WriteLine($"Error during streaming: {ex}");
                throw;
            }

            tokens.Should().NotBeEmpty("No tokens were received from the stream");
            string.Concat(tokens).Should().NotBeNullOrEmpty("Combined token text should not be empty");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Generate_WithStopSequence_ShouldGenerateText()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Arrange
            KoboldSharpRequest request = new KoboldSharpRequest
            {
                Prompt = "Write a short story",
                MaxLength = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                StopSequences = new List<string> { "." }
            };

            // Act
            KoboldSharpResponse response = await Provider.GenerateAsync(request);

            // Assert
            response.Should().NotBeNull();
            response.Results.Should().NotBeEmpty();
            response.Results[0].Text.Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Generate_WithDifferentTemperatures_ShouldWork()
        {
            Skip.If(!ServerAvailable, "KoboldCpp server is not available");

            // Test various temperature settings
            float[] temperatures = new[] { 0.1f, 0.7f, 1.5f };
            foreach (float temp in temperatures)
            {
                // Arrange
                KoboldSharpRequest request = new KoboldSharpRequest
                {
                    Prompt = "The quick brown fox",
                    MaxLength = 20,
                    Temperature = temp,
                    TopP = 0.9f,
                    TopK = 40,
                    RepetitionPenalty = 1.1f,
                    RepetitionPenaltyRange = 64
                };

                // Act
                KoboldSharpResponse response = await Provider.GenerateAsync(request);

                // Assert
                response.Should().NotBeNull();
                response.Results.Should().NotBeEmpty();
                response.Results[0].Text.Should().NotBeNullOrEmpty();
                Output.WriteLine($"Temperature {temp} response: {response.Results[0].Text}");
                
                // Those two temperature values produce unique enough outputs (often shorter or more abrupt) that cause the server’s final SSE chunk or the socket close to happen in a way .NET perceives as “premature.”
                // It’s a known type of SSE/chunked-encoding edge case—not actually a model crash or a timeout. It just tends to surface with certain generation lengths or timing patterns, which happen more at extremes like 0.1 and 1.5.
                await Task.Delay(500);
            }
        }
    }
}