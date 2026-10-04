using AiProviderBenchmarker.Application.UseCases;
using AiProviderBenchmarker.Cli.Arguments;
using AiProviderBenchmarker.Cli.UI;
using AiProviderBenchmarker.Domain.Model;
using AiProviderBenchmarker.Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace AiProviderBenchmarker.Cli.Runners;

/// <summary>
/// Executor responsável pelo modo autônomo (não-interativo), ideal para CI/CD e automação.
/// </summary>
public class NonInteractiveBenchmarkRunner : IBenchmarkRunner
{
    private readonly IRunBenchmarkUseCase _useCase;
    private readonly AiProvidersOptions _options;

    public NonInteractiveBenchmarkRunner(
        IRunBenchmarkUseCase useCase,
        IOptions<AiProvidersOptions> options)
    {
        _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
        _options = options?.Value ?? new AiProvidersOptions();
    }

    public async Task<int> RunAsync(CliParsedOptions parsedOptions, CancellationToken cancellationToken)
    {
        TableRenderer.RenderHeader();
        TableRenderer.RenderProviderStatus(_options);

        var providers = parsedOptions.Providers.Count > 0 
            ? parsedOptions.Providers.ToList() 
            : ResolveDefaultProviders();

        AnsiConsole.MarkupLine($"[bold green]Executando em modo não-interativo com {providers.Count} provedor(es)...[/]");

        var command = new RunBenchmarkCommand(
            Prompt: parsedOptions.Prompt,
            Options: new ChatOptions { MaxOutputTokens = parsedOptions.MaxTokens },
            ProvidersToBenchmark: providers);

        var results = await _useCase.ExecuteAsync(command, streamProgress: null, cancellationToken: cancellationToken);
        TableRenderer.RenderResultsTable(results, parsedOptions.Prompt);

        return 0;
    }

    private List<string> ResolveDefaultProviders()
    {
        var list = _options.Providers
            .Where(p => p.IsConfigured)
            .Select(p => p.Key)
            .ToList();

        var sim = _options.GetProvider("Simulated");
        var simKey = sim != null ? sim.Key : "Simulated";

        if (list.Count == 0 || (!list.Contains("Simulated", StringComparer.OrdinalIgnoreCase) && !list.Contains(simKey, StringComparer.OrdinalIgnoreCase)))
        {
            list.Insert(0, simKey);
        }

        return list;
    }
}
