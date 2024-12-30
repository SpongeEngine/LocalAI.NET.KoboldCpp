using FluentAssertions;
using LocalAI.NET.KoboldCpp.Providers.OpenAi;
using Xunit;
using Xunit.Abstractions;

namespace LocalAI.NET.KoboldCpp.Tests.Integration.Providers.OpenAi
{
    [Trait("Category", "Integration")]
    [Trait("API", "OpenAI")]
    public class OpenAiCompletionTests : OpenAiTestBase
    {
        public OpenAiCompletionTests(ITestOutputHelper output) : base(output) { }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Complete_WithSimplePrompt_ShouldWork()
        {
            Skip.If(!ServerAvailable, "OpenAI endpoint not available");

            // Arrange & Act
            var response = await Provider.CompleteAsync(
                "Once upon a time",
                new CompletionOptions
                {
                    MaxTokens = 20,
                    Temperature = 0.7f,
                    TopP = 0.9f
                });

            // Assert
            response.Should().NotBeNullOrEmpty();
            Output.WriteLine($"Completion response: {response}");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Complete_WithStopSequence_ShouldWork()
        {
            Skip.If(!ServerAvailable, "OpenAI endpoint not available");

            // Arrange & Act
            var response = await Provider.CompleteAsync(
                "Write a short story",
                new CompletionOptions
                {
                    MaxTokens = 20,
                    Temperature = 0.7f,
                    TopP = 0.9f,
                    StopSequences = new[] { "." }
                });

            // Assert
            response.Should().NotBeNullOrEmpty();
            Output.WriteLine($"Completion response: {response}");
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task Complete_WithDifferentTemperatures_ShouldWork()
        {
            Skip.If(!ServerAvailable, "OpenAI endpoint not available");

            var temperatures = new[] { 0.1f, 0.7f, 1.5f };
            foreach (var temp in temperatures)
            {
                // Arrange & Act
                var response = await Provider.CompleteAsync(
                    "The quick brown fox",
                    new CompletionOptions
                    {
                        MaxTokens = 20,
                        Temperature = temp,
                        TopP = 0.9f
                    });

                // Assert
                response.Should().NotBeNullOrEmpty();
                Output.WriteLine($"Temperature {temp} response: {response}");
                
                // Those two temperature values produce unique enough outputs (often shorter or more abrupt) that cause the server’s final SSE chunk or the socket close to happen in a way .NET perceives as “premature.”
                // It’s a known type of SSE/chunked-encoding edge case—not actually a model crash or a timeout. It just tends to surface with certain generation lengths or timing patterns, which happen more at extremes like 0.1 and 1.5.
                await Task.Delay(500);
            }
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task StreamCompletion_ShouldStreamTokens()
        {
            Skip.If(!ServerAvailable, "OpenAI endpoint not available");

            // Arrange
            var options = new CompletionOptions
            {
                MaxTokens = 20,
                Temperature = 0.7f,
                TopP = 0.9f
            };

            // Act
            var tokens = new List<string>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                await foreach (var token in Provider.StreamCompletionAsync(
                    "Write a short story about",
                    options,
                    cts.Token))
                {
                    tokens.Add(token);
                    Output.WriteLine($"Received token: {token}");

                    if (tokens.Count >= options.MaxTokens)
                        break;
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                Output.WriteLine($"Stream timed out after receiving {tokens.Count} tokens");
            }

            // Assert
            tokens.Should().NotBeEmpty();
            string.Concat(tokens).Should().NotBeNullOrEmpty();
        }

        [SkippableFact]
        [Trait("Category", "Integration")]
        public async Task StreamCompletion_WithStopSequence_ShouldWork()
        {
            Skip.If(!ServerAvailable, "OpenAI endpoint not available");

            // Arrange
            var options = new CompletionOptions
            {
                MaxTokens = 20,
                Temperature = 0.7f,
                TopP = 0.9f,
                StopSequences = new[] { "." }
            };

            // Act
            var tokens = new List<string>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                await foreach (var token in Provider.StreamCompletionAsync(
                    "Write a short story about",
                    options,
                    cts.Token))
                {
                    tokens.Add(token);
                    Output.WriteLine($"Received token: {token}");

                    if (tokens.Count >= options.MaxTokens)
                        break;
                }
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested)
            {
                Output.WriteLine($"Stream timed out after receiving {tokens.Count} tokens");
            }

            // Assert
            tokens.Should().NotBeEmpty();
            string.Concat(tokens).Should().NotBeNullOrEmpty();
        }
    }
}