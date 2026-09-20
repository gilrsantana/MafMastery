using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Application.UseCases;

/// <summary>
/// DTO com os parâmetros necessários para execução do benchmark de IA.
/// </summary>
public record RunBenchmarkCommand(
    string Prompt,
    ChatOptions Options,
    IReadOnlyList<string> ProvidersToBenchmark);
