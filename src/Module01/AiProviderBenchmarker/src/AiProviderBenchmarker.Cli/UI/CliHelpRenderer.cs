using Spectre.Console;

namespace AiProviderBenchmarker.Cli.UI;

/// <summary>
/// Renderer for command-line documentation and help screens.
/// </summary>
public static class CliHelpRenderer
{
    public static void Render()
    {
        AnsiConsole.MarkupLine("[bold cyan]AiProviderBenchmarker — AI Benchmark & SLA CLI[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Interactive usage:[/] [green]dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli[/]");
        AnsiConsole.MarkupLine("[bold]Command-line usage:[/] [green]dotnet run --project src/Module01/AiProviderBenchmarker/src/AiProviderBenchmarker.Cli -- [[options]][/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold yellow]Available options:[/]");
        AnsiConsole.MarkupLine("  [yellow]-p, --prompt <text>[/]        Evaluation prompt to send");
        AnsiConsole.MarkupLine("  [yellow]--providers <list>[/]         Comma-separated providers (e.g., Simulated,OpenAi,AzureOpenAi,Ollama)");
        AnsiConsole.MarkupLine("  [yellow]-m, --max-tokens <num>[/]     Maximum output tokens generated (default: 300)");
        AnsiConsole.MarkupLine("  [yellow]-h, --help[/]                 Display this help message");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Note: When using 'dotnet run', separate application arguments using '--'. Example:[/]");
        AnsiConsole.MarkupLine("[grey]  dotnet run --project .../AiProviderBenchmarker.Cli -- --providers Ollama,Simulated --max-tokens 200[/]");
        AnsiConsole.WriteLine();
    }
}
