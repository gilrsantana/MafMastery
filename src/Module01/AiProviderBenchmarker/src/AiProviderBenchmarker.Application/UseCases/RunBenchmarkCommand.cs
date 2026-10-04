using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Application.UseCases;

/// <summary>
/// Command DTO containing parameters for running an AI benchmark.
/// </summary>
public record RunBenchmarkCommand(
    string Prompt,
    ChatOptions Options,
    IReadOnlyList<string> ProvidersToBenchmark);
