using AiProviderBenchmarker.Domain.Model;

namespace AiProviderBenchmarker.Application.UseCases;

/// <summary>
/// Use case contract for executing and benchmarking multiple AI providers concurrently.
/// </summary>
public interface IRunBenchmarkUseCase
{
    Task<IReadOnlyList<ProviderMetric>> ExecuteAsync(
        RunBenchmarkCommand command,
        IProgress<(string Provider, string Chunk)>? streamProgress = null,
        CancellationToken cancellationToken = default);
}
