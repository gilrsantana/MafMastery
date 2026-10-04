using AiProviderBenchmarker.Cli.Arguments;
using AiProviderBenchmarker.Cli.UI;

namespace AiProviderBenchmarker.Cli.Runners;

/// <summary>
/// Main CLI application orchestrator. Determines the execution mode and dispatches to the appropriate runner.
/// </summary>
public class CliApp
{
    private readonly CliArgumentParser _argumentParser;
    private readonly InteractiveBenchmarkRunner _interactiveRunner;
    private readonly NonInteractiveBenchmarkRunner _nonInteractiveRunner;

    public CliApp(
        CliArgumentParser argumentParser,
        InteractiveBenchmarkRunner interactiveRunner,
        NonInteractiveBenchmarkRunner nonInteractiveRunner)
    {
        _argumentParser = argumentParser ?? throw new ArgumentNullException(nameof(argumentParser));
        _interactiveRunner = interactiveRunner ?? throw new ArgumentNullException(nameof(interactiveRunner));
        _nonInteractiveRunner = nonInteractiveRunner ?? throw new ArgumentNullException(nameof(nonInteractiveRunner));
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var options = _argumentParser.Parse(args);

        if (options.IsHelp)
        {
            CliHelpRenderer.Render();
            return 0;
        }

        if (options.IsNonInteractive)
        {
            return await _nonInteractiveRunner.RunAsync(options, cancellationToken);
        }

        return await _interactiveRunner.RunAsync(options, cancellationToken);
    }
}
