# KoboldSharp
[![NuGet](https://img.shields.io/nuget/v/SpongeEngine.KoboldSharp.svg)](https://www.nuget.org/packages/SpongeEngine.KoboldSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SpongeEngine.KoboldSharp.svg)](https://www.nuget.org/packages/SpongeEngine.KoboldSharp)
[![Run Tests](https://github.com/SpongeEngine/KoboldSharp/actions/workflows/run-tests.yml/badge.svg)](https://github.com/SpongeEngine/KoboldSharp/actions/workflows/run-tests.yml)
[![License](https://img.shields.io/github/license/SpongeEngine/KoboldSharp)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0%2B-512BD4)](https://dotnet.microsoft.com/download)

C# client for interacting with KoboldCpp through its native and OpenAI-compatible endpoints.

## Features
- Complete support for KoboldCpp's native API
- OpenAI-compatible API endpoint support
- Streaming text generation
- Comprehensive configuration options
- Built-in error handling and logging
- Cross-platform compatibility
- Full async/await support

📦 [View Package on NuGet](https://www.nuget.org/packages/SpongeEngine.KoboldSharp)

## Installation
Install via NuGet:
```bash
dotnet add package SpongeEngine.KoboldSharp
```

## Quick Start

### Using Native API
```csharp
using SpongeEngine.KoboldSharp.Client;
using SpongeEngine.KoboldSharp.Models;

// Configure the client
var options = new KoboldSharpOptions
{
    BaseUrl = "http://localhost:5001",
    UseGpu = true,
    ContextSize = 2048
};

// Create client instance
using var client = new KoboldSharpClient(options);

// Generate completion
var request = new KoboldSharpRequest
{
    Prompt = "Write a short story about a robot:",
    MaxLength = 200,
    Temperature = 0.7f,
    TopP = 0.9f
};

var response = await client.GenerateAsync(request);
Console.WriteLine(response.Results[0].Text);

// Stream completion
await foreach (var token in client.GenerateStreamAsync(request))
{
    Console.Write(token);
}
```

### Using OpenAI-Compatible API
```csharp
var options = new KoboldSharpOptions
{
    BaseUrl = "http://localhost:5001",
    UseOpenAiApi = true
};

using var client = new KoboldSharpClient(options);

// Simple completion
string response = await client.CompleteAsync(
    "Write a short story about:",
    new CompletionOptions
    {
        MaxTokens = 200,
        Temperature = 0.7f,
        TopP = 0.9f
    });

// Stream completion
await foreach (var token in client.StreamCompletionAsync(
    "Once upon a time...",
    new CompletionOptions { MaxTokens = 200 }))
{
    Console.Write(token);
}
```

## Configuration Options

### Basic Options
```csharp
var options = new KoboldSharpOptions
{
    BaseUrl = "http://localhost:5001",    // KoboldCpp server URL
    ApiKey = "optional_api_key",          // Optional API key
    TimeoutSeconds = 600,                 // Request timeout
    ContextSize = 2048,                   // Maximum context size
    UseGpu = true,                        // Enable GPU acceleration
    UseOpenAiApi = false                  // Use OpenAI-compatible API
};
```

### Advanced Generation Parameters
```csharp
var request = new KoboldSharpRequest
{
    Prompt = "Your prompt here",
    MaxLength = 200,                      // Maximum tokens to generate
    MaxContextLength = 2048,              // Maximum context length
    Temperature = 0.7f,                   // Randomness (0.0-1.0)
    TopP = 0.9f,                         // Nucleus sampling threshold
    TopK = 40,                           // Top-K sampling
    TopA = 0.0f,                         // Top-A sampling
    Typical = 1.0f,                      // Typical sampling
    Tfs = 1.0f,                          // Tail-free sampling
    RepetitionPenalty = 1.1f,            // Repetition penalty
    RepetitionPenaltyRange = 64,         // Penalty range
    StopSequences = new List<string> { "\n" },  // Stop sequences
    Stream = false,                       // Enable streaming
    TrimStop = true,                      // Trim stop sequences
    MirostatMode = 0,                     // Mirostat sampling mode
    MirostatTau = 5.0f,                   // Mirostat target entropy
    MirostatEta = 0.1f                    // Mirostat learning rate
};
```

## Error Handling
```csharp
try
{
    var response = await client.GenerateAsync(request);
}
catch (KoboldSharpException ex)
{
    Console.WriteLine($"KoboldCpp error: {ex.Message}");
    Console.WriteLine($"Provider: {ex.Provider}");
    if (ex.StatusCode.HasValue)
    {
        Console.WriteLine($"Status code: {ex.StatusCode}");
    }
    if (ex.ResponseContent != null)
    {
        Console.WriteLine($"Response content: {ex.ResponseContent}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"General error: {ex.Message}");
}
```

## Logging
The client supports Microsoft.Extensions.Logging:

```csharp
ILogger logger = LoggerFactory
    .Create(builder => builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Debug))
    .CreateLogger<KoboldSharpClient>();

var client = new KoboldSharpClient(options, logger);
```

## JSON Serialization
Custom JSON settings can be provided:

```csharp
var jsonSettings = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore
};

var client = new KoboldSharpClient(options, jsonSettings: jsonSettings);
```

## Testing
The library includes both unit and integration tests. Integration tests require a running KoboldCpp server.

To run the tests:
```bash
dotnet test
```

To configure the test environment:
```csharp
// Set environment variables for testing
Environment.SetEnvironmentVariable("KOBOLDCPP_BASE_URL", "http://localhost:5001");
Environment.SetEnvironmentVariable("KOBOLDCPP_OPENAI_BASE_URL", "http://localhost:5001/v1");
```

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing
Contributions are welcome! Please feel free to submit a Pull Request.

## Support
For issues and feature requests, please use the [GitHub issues page](https://github.com/SpongeEngine/KoboldSharp/issues).
