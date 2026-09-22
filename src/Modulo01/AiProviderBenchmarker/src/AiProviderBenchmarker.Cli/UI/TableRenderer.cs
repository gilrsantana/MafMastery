using AiProviderBenchmarker.Domain.Model;
using AiProviderBenchmarker.Infrastructure.Configuration;
using Spectre.Console;

namespace AiProviderBenchmarker.Cli.UI;

public static class TableRenderer
{
    public static void RenderHeader()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(
            new FigletText("MafMastery")
                .Color(Color.Cyan1));

        var panel = new Panel(
            new Markup("[bold yellow]Módulo 01:[/] [white]AiProviderBenchmarker — Comparador Concorrente de Provedores de IA[/]\n" +
                       "[grey]Baseado em Microsoft.Extensions.AI (MEAI) | .NET 10 LTS | Clean Architecture[/]"))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.SteelBlue)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    public static void RenderResultsTable(IReadOnlyList<ProviderMetric> results, string prompt)
    {
        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nenhum resultado de benchmark coletado.[/]");
            return;
        }

        AnsiConsole.WriteLine();
        var promptPreview = prompt.Length > 80 ? prompt[..77] + "..." : prompt;
        AnsiConsole.MarkupLine($"[grey]Prompt avaliado:[/] [italic white]\"{Markup.Escape(promptPreview)}\"[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold cyan]📊 Relatório Consolidado de SLA e FinOps[/]");

        table.AddColumn(new TableColumn("[bold]Provedor / Modelo[/]"));
        table.AddColumn(new TableColumn("[bold]Status[/]").Centered());
        table.AddColumn(new TableColumn("[bold]TTFT[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Latência[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Tokens E/S[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]TPS (Tokens/s)[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Custo ($ USD)[/]").RightAligned());

        var successfulResults = results.Where(r => r.Success).ToList();
        var lowestTtft = successfulResults.Count > 0 ? successfulResults.Min(r => r.TimeToFirstToken) : TimeSpan.Zero;
        var highestTps = successfulResults.Count > 0 ? successfulResults.Max(r => r.TokensPerSecond) : 0;
        var lowestCost = successfulResults.Count > 0 ? successfulResults.Min(r => r.EstimatedCostUsd) : 0;

        foreach (var metric in results)
        {
            var providerName = $"[bold white]{metric.Provider}[/]\n[grey]{metric.ModelName}[/]";
            var status = metric.Success 
                ? "[green]✔ OK[/]" 
                : "[red]✖ Falha[/]";

            if (!metric.Success)
            {
                table.AddRow(
                    providerName,
                    status,
                    "[grey]-[/]",
                    $"{metric.TotalDuration.TotalSeconds:F2}s",
                    "[grey]-[/]",
                    "[grey]-[/]",
                    "[grey]$0.00[/]");
                continue;
            }

            // TTFT highlight
            var ttftMs = metric.TimeToFirstToken.TotalMilliseconds;
            var ttftColor = metric.TimeToFirstToken == lowestTtft ? "bold green" : "white";
            var ttftText = $"[{ttftColor}]{ttftMs:F0} ms[/]";

            // Latency
            var latencyText = $"{metric.TotalDuration.TotalSeconds:F2} s";

            // Tokens
            var tokensText = $"[grey]{metric.InputTokens}[/] / [white]{metric.OutputTokens}[/]";

            // TPS highlight
            var tpsColor = metric.TokensPerSecond == highestTps ? "bold green" : (metric.TokensPerSecond > 30 ? "cyan" : "white");
            var tpsText = $"[{tpsColor}]{metric.TokensPerSecond:F1}[/]";

            // Cost highlight
            var costColor = metric.EstimatedCostUsd == lowestCost ? "bold green" : "yellow";
            var costText = $"[{costColor}]${metric.EstimatedCostUsd:F6}[/]";

            table.AddRow(
                providerName,
                status,
                ttftText,
                latencyText,
                tokensText,
                tpsText,
                costText);
        }

        AnsiConsole.Write(table);

        // Renderiza Snippets de Resposta
        AnsiConsole.WriteLine();
        var tree = new Tree("[bold blue]📝 Prévia das Respostas Geradas[/]");
        foreach (var metric in results)
        {
            if (metric.Success)
            {
                var node = tree.AddNode($"[bold]{metric.Provider}[/] ([cyan]{metric.ModelName}[/])");
                node.AddNode(new Markup($"[italic grey]{Markup.Escape(metric.GeneratedTextSnippet)}[/]"));
            }
            else
            {
                var errorNode = tree.AddNode($"[bold red]{metric.Provider}[/] (Falha)");
                errorNode.AddNode(new Markup($"[red]Erro: {Markup.Escape(metric.ErrorMessage ?? "Desconhecido")}[/]"));
            }
        }
        AnsiConsole.Write(tree);

        // Vencedores do Benchmark
        if (successfulResults.Count > 1)
        {
            AnsiConsole.WriteLine();
            var winnerTtft = successfulResults.First(r => r.TimeToFirstToken == lowestTtft);
            var winnerTps = successfulResults.First(r => r.TokensPerSecond == highestTps);
            var winnerCost = successfulResults.First(r => r.EstimatedCostUsd == lowestCost);

            var summaryGrid = new Grid();
            summaryGrid.AddColumn();
            summaryGrid.AddColumn();
            summaryGrid.AddColumn();

            summaryGrid.AddRow(
                new Panel($"[bold green]🚀 Menor TTFT[/]\n[white]{winnerTtft.Provider}[/] [cyan]({winnerTtft.ModelName})[/]\n({winnerTtft.TimeToFirstToken.TotalMilliseconds:F0} ms)") { Border = BoxBorder.Rounded },
                new Panel($"[bold green]⚡ Maior Throughput[/]\n[white]{winnerTps.Provider}[/] [cyan]({winnerTps.ModelName})[/]\n({winnerTps.TokensPerSecond:F1} tokens/s)") { Border = BoxBorder.Rounded },
                new Panel($"[bold green]💰 Mais Econômico[/]\n[white]{winnerCost.Provider}[/] [cyan]({winnerCost.ModelName})[/]\n(${winnerCost.EstimatedCostUsd:F6})") { Border = BoxBorder.Rounded }
            );

            AnsiConsole.Write(summaryGrid);
        }
    }

    public static void RenderProviderStatus(AiProvidersOptions options)
    {
        var statusGrid = new Grid();
        statusGrid.AddColumn();
        statusGrid.AddColumn();
        statusGrid.AddColumn();
        statusGrid.AddColumn();

        static string FormatStatus(string name, bool isConfigured, string details) =>
            isConfigured
                ? $"[bold green]✔ {name}[/]\n[grey]{details}[/]"
                : $"[grey]○ {name}[/]\n[italic darkorange3]{details}[/]";

        var formatted = options.Providers.Select(p => 
            FormatStatus(p.AiProviderName, p.IsConfigured, p.IsConfigured ? p.DeploymentName : "Chave ausente (Fallback)")
        ).ToList();

        for (var i = 0; i < formatted.Count; i += 4)
        {
            var chunk = formatted.Skip(i).Take(4).ToArray();
            statusGrid.AddRow(chunk);
        }
        
        // var openAi = options.GetProvider("OpenAi");
        // var azure = options.GetProvider("AzureOpenAi");
        // var ollama = options.GetProvider("Ollama");
        // var sim = options.GetProvider("Simulated");
        //
        // statusGrid.AddRow(
        //     FormatStatus("OpenAI", openAi is { IsConfigured: true }, openAi is { IsConfigured: true } ? openAi.DeploymentName : "Chave ausente (Fallback Simulado)"),
        //     FormatStatus("Azure OpenAI", azure is { IsConfigured: true }, azure is { IsConfigured: true } ? azure.DeploymentName : "Endpoint ausente (Fallback)"),
        //     FormatStatus("Ollama (Local)", ollama is { IsConfigured: true }, ollama is { IsConfigured: true } ? ollama.DeploymentName : "Ollama desabilitado"),
        //     FormatStatus("Simulado (Offline)", sim?.Enabled ?? true, !string.IsNullOrWhiteSpace(sim?.DeploymentName) ? sim.DeploymentName : "simulated-fast-llm")
        // );

        var panel = new Panel(statusGrid)
        {
            Header = new PanelHeader("[bold white]Provedores Detectados no Ambiente[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey37)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }
}
