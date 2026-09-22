using Spectre.Console;

namespace AiProviderBenchmarker.Cli.UI;

/// <summary>
/// Renderizador da documentação e tela de ajuda via linha de comando.
/// </summary>
public static class CliHelpRenderer
{
    public static void Render()
    {
        AnsiConsole.MarkupLine("[bold cyan]AiProviderBenchmarker — CLI de Benchmark e SLA de IA[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Uso interativo:[/] [green]dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli[/]");
        AnsiConsole.MarkupLine("[bold]Uso com parâmetros:[/] [green]dotnet run --project src/Modulo01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- [[opções]][/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Opções disponíveis:[/]");
        AnsiConsole.MarkupLine("  [yellow]-p, --prompt <texto>[/]       Prompt de avaliação a ser enviado");
        AnsiConsole.MarkupLine("  [yellow]--providers <lista>[/]        Provedores separados por vírgula (Ex: Simulated,OpenAi,AzureOpenAi,Ollama)");
        AnsiConsole.MarkupLine("  [yellow]-m, --max-tokens <num>[/]     Quantidade máxima de tokens gerados (default: 300)");
        AnsiConsole.MarkupLine("  [yellow]-h, --help[/]                 Exibe esta mensagem de ajuda");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Nota: Ao usar 'dotnet run', separe os argumentos da sua aplicação usando '--'. Exemplo:[/]");
        AnsiConsole.MarkupLine("[grey]  dotnet run --project .../AiProviderBenchmarker.Cli -- --providers Ollama,Simulated --max-tokens 200[/]");
        AnsiConsole.WriteLine();
    }
}
