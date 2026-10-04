using AiProviderBenchmarker.Cli.Arguments;

namespace AiProviderBenchmarker.Cli.Runners;

/// <summary>
/// Contract for benchmark runners in the CLI application.
/// </summary>
public interface IBenchmarkRunner
{
    Task<int> RunAsync(CliParsedOptions options, CancellationToken cancellationToken);
}
