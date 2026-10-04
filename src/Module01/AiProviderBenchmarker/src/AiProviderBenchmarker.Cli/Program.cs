using System.Text;
using AiProviderBenchmarker.Cli.Configuration;
using AiProviderBenchmarker.Cli.Runners;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace AiProviderBenchmarker.Cli;

/// <summary>
/// Minimal application entrypoint.
/// Responsible solely for bootstrapping environment, cancellation capture, and triggering the IoC container.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
            AnsiConsole.MarkupLine("[bold red]\nOperation cancelled by user (Ctrl+C).[/]");
        };

        var configuration = ServiceCollectionExtensions.BuildAppConfiguration();
        var services = new ServiceCollection()
            .AddCliServices(configuration)
            .BuildServiceProvider();

        await using (services)
        {
            var app = services.GetRequiredService<CliApp>();
            return await app.RunAsync(args, cts.Token);
        }
    }
}
