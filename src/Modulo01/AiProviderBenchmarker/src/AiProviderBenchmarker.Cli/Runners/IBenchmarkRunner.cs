using AiProviderBenchmarker.Cli.Arguments;

namespace AiProviderBenchmarker.Cli.Runners;

/// <summary>
/// Contrato para os executores de benchmark da aplicação CLI.
/// </summary>
public interface IBenchmarkRunner
{
    Task<int> RunAsync(CliParsedOptions options, CancellationToken cancellationToken);
}
