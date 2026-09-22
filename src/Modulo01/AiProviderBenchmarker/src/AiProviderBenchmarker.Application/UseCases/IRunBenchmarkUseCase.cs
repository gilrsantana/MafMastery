using AiProviderBenchmarker.Domain.Model;

namespace AiProviderBenchmarker.Application.UseCases;

/// <summary>
/// Contrato do caso de uso de execução e medição do benchmark simultâneo.
/// </summary>
public interface IRunBenchmarkUseCase
{
    Task<IReadOnlyList<ProviderMetric>> ExecuteAsync(
        RunBenchmarkCommand command,
        IProgress<(string Provider, string Chunk)>? streamProgress = null,
        CancellationToken cancellationToken = default);
}
