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
/// Runner responsible for the interactive guided terminal experience.
/// </summary>
public class InteractiveBenchmarkRunner : IBenchmarkRunner
{
    private readonly IRunBenchmarkUseCase _useCase;
    private readonly AiProvidersOptions _options;

    public InteractiveBenchmarkRunner(
        IRunBenchmarkUseCase useCase,
        IOptions<AiProvidersOptions> options)
    {
        _useCase = useCase ?? throw new ArgumentNullException(nameof(useCase));
        _options = options?.Value ?? new AiProvidersOptions();
    }

    public async Task<int> RunAsync(CliParsedOptions parsedOptions, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TableRenderer.RenderHeader();
            TableRenderer.RenderProviderStatus(_options);

            // 1. Select Prompt
            var prompt = SelectPrompt();
            if (string.IsNullOrWhiteSpace(prompt))
                break;

            // 2. Select Providers
            var selectedProviders = SelectProviders(_options);
            if (selectedProviders.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No provider selected. Aborting round.[/]");
                if (!AskToRepeat()) break;
                continue;
            }

            // 3. Max Tokens Limit
            var maxTokens = AnsiConsole.Prompt(
                new TextPrompt<int>("[cyan]Maximum output tokens limit (max_tokens):[/]")
                    .DefaultValue(300)
                    .ValidationErrorMessage("[red]Please enter a valid positive integer (min: 1 - max: 4096).[/]")
                    .Validate(val => val is > 0 and <= 4096));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold green]Starting concurrent benchmark with {selectedProviders.Count} provider(s)...[/]");

            var command = new RunBenchmarkCommand(
                Prompt: prompt,
                Options: new ChatOptions { MaxOutputTokens = maxTokens },
                ProvidersToBenchmark: selectedProviders);

            IReadOnlyList<ProviderMetric> results = [];

            try
            {
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .SpinnerStyle(Style.Parse("yellow bold"))
                    .StartAsync("Dispatching parallel requests and measuring TTFT...", async ctx =>
                    {
                        results = await _useCase.ExecuteAsync(command, streamProgress: null, cancellationToken: cancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Execution cancelled by user.[/]");
                break;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Fatal error during benchmark execution:[/] {Markup.Escape(ex.Message)}");
            }

            // Render Results
            TableRenderer.RenderResultsTable(results, prompt);

            // Prompt to repeat
            if (!AskToRepeat())
            {
                break;
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Thank you for using AiProviderBenchmarker! See you next time.[/]");
        return 0;
    }

    private static string SelectPrompt()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold yellow]Select a prompt scenario for the benchmark:[/]\n")
                .PageSize(6)
                .AddChoices([
                    "1. C# Code (.NET 10 & Binary Search)",
                    "2. Architectural Analysis (Microservices vs Modular Monolith)",
                    "3. Technical Translation & Summary (Streaming & TTFT in LLMs)",
                    "4. Custom Prompt (Type freely)",
                    "5. Exit application"
                ]));

        return choice switch
        {
            "1. C# Code (.NET 10 & Binary Search)" =>
                "Write a C# 14 (.NET 10) function that calculates Fibonacci using binary search on a precomputed sequence with proper exception handling.",

            "2. Architectural Analysis (Microservices vs Modular Monolith)" =>
                "Explain the architectural trade-offs between microservices and modular monoliths in the .NET ecosystem, considering Clean Architecture and infrastructure costs.",

            "3. Technical Translation & Summary (Streaming & TTFT in LLMs)" =>
                "Translate and summarize the following technical paragraph regarding streaming tokens in LLMs: 'Streaming tokens allow users to view partial outputs in real time, drastically reducing perceived latency and improving Time To First Token (TTFT).'",

            "4. Custom Prompt (Type freely)" =>
                AnsiConsole.Ask<string>("[green]Enter your test prompt:[/]\n> "),

            _ => string.Empty
        };
    }

    private static IReadOnlyList<string> SelectProviders(AiProvidersOptions options)
    {
        var prompt = new MultiSelectionPrompt<string>()
            .Title("[bold yellow]Select providers to benchmark in this round:[/]")
            .PageSize(10)
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to confirm)[/]")
            .UseConverter(key =>
            {
                var cfg = options.GetProvider(key);
                if (cfg is null) return Markup.Escape(key);
                if (cfg.AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase))
                    return $"Simulated ({Markup.Escape(cfg.DeploymentName)}) [green](Offline / Deterministic Mock)[/]";
                if (cfg.IsConfigured)
                    return $"{Markup.Escape(cfg.AiProviderName)} ({Markup.Escape(cfg.DeploymentName)}) [green](Active)[/]";
                return $"{Markup.Escape(cfg.AiProviderName)} ({Markup.Escape(cfg.DeploymentName)}) [yellow](Simulated Fallback)[/]";
            });

        foreach (var p in options.Providers)
        {
            prompt.AddChoice(p.Key);
        }

        return AnsiConsole.Prompt(prompt);
    }

    private static bool AskToRepeat()
    {
        AnsiConsole.WriteLine();
        return AnsiConsole.Confirm("[bold yellow]Would you like to run another benchmark?[/]", defaultValue: true);
    }
}
