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
/// Executor responsável pela experiência interativa guiada no terminal.
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

            // 1. Escolha do Prompt
            var prompt = SelectPrompt();
            if (string.IsNullOrWhiteSpace(prompt))
                break;

            // 2. Escolha dos Provedores
            var selectedProviders = SelectProviders(_options);
            if (selectedProviders.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nenhum provedor selecionado. Abortando rodada.[/]");
                if (!AskToRepeat()) break;
                continue;
            }

            // 3. Limite de Tokens
            var maxTokens = AnsiConsole.Prompt(
                new TextPrompt<int>("[cyan]Limite máximo de tokens de saída (max_tokens):[/]")
                    .DefaultValue(300)
                    .ValidationErrorMessage("[red]Por favor informe um número inteiro positivo válido. (min:1 - max: 4096)[/]")
                    .Validate(val => val is > 0 and <= 4096));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold green]Iniciando benchmark concorrente com {selectedProviders.Count} provedor(es)...[/]");

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
                    .StartAsync("Disparando requisições em paralelo e mensurando TTFT...", async ctx =>
                    {
                        results = await _useCase.ExecuteAsync(command, streamProgress: null, cancellationToken: cancellationToken);
                    });
            }
            catch (OperationCanceledException)
            {
                AnsiConsole.MarkupLine("[yellow]Execução interrompida via cancelamento.[/]");
                break;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Erro fatal na execução do benchmark:[/] {Markup.Escape(ex.Message)}");
            }

            // Renderiza Resultados
            TableRenderer.RenderResultsTable(results, prompt);

            // Pergunta se deseja repetir
            if (!AskToRepeat())
            {
                break;
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Obrigado por utilizar o AiProviderBenchmarker! Até a próxima rodada.[/]");
        return 0;
    }

    private static string SelectPrompt()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold yellow]Selecione o cenário de prompt para o benchmark:[/]\n")
                .PageSize(6)
                .AddChoices([
                    "1. Código C# (.NET 10 & Busca Binária)",
                    "2. Análise Arquitetural (Microserviços vs Monólito Modular)",
                    "3. Tradução & Resumo Técnico (Streaming & TTFT em LLMs)",
                    "4. Prompt Personalizado (Digitar livremente)",
                    "5. Sair da aplicação"
                ]));

        return choice switch
        {
            "1. Código C# (.NET 10 & Busca Binária)" =>
                "Escreva uma função em C# 14 (.NET 10) que calcula Fibonacci usando busca binária em uma sequência pré-computada e trate exceções.",

            "2. Análise Arquitetural (Microserviços vs Monólito Modular)" =>
                "Explique os trade-offs arquiteturais entre microserviços e monólitos modulares no ecossistema .NET, considerando Clean Architecture e custos de infraestrutura.",

            "3. Tradução & Resumo Técnico (Streaming & TTFT em LLMs)" =>
                "Traduza e resuma o seguinte parágrafo técnico sobre streaming tokens em LLMs: 'Streaming tokens allow users to view partial outputs in real time, drastically reducing perceived latency and improving Time To First Token (TTFT).'",

            "4. Prompt Personalizado (Digitar livremente)" =>
                AnsiConsole.Ask<string>("[green]Digite seu prompt de teste:[/]\n> "),

            _ => string.Empty
        };
    }

    private static IReadOnlyList<string> SelectProviders(AiProvidersOptions options)
    {
        var prompt = new MultiSelectionPrompt<string>()
            .Title("[bold yellow]Selecione os provedores para competir neste benchmark:[/]")
            .PageSize(10)
            .InstructionsText("[grey](Pressione [blue]<espaço>[/] para marcar/desmarcar, [green]<enter>[/] para confirmar)[/]")
            .UseConverter(key =>
            {
                var cfg = options.GetProvider(key);
                if (cfg is null) return Markup.Escape(key);
                if (cfg.AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase))
                    return $"Simulated ({Markup.Escape(cfg.DeploymentName)}) [green](Offline / Mock Determinístico)[/]";
                if (cfg.IsConfigured)
                    return $"{Markup.Escape(cfg.AiProviderName)} ({Markup.Escape(cfg.DeploymentName)}) [green](Ativo)[/]";
                return $"{Markup.Escape(cfg.AiProviderName)} ({Markup.Escape(cfg.DeploymentName)}) [yellow](Fallback Simulado)[/]";
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
        return AnsiConsole.Confirm("[bold yellow]Deseja executar outro benchmark?[/]", defaultValue: true);
    }
}
