using System.Text;
using AiProviderBenchmarker.Cli.Configuration;
using AiProviderBenchmarker.Cli.Runners;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace AiProviderBenchmarker.Cli;

/// <summary>
/// Ponto de entrada (Entrypoint) minimalista da aplicação.
/// Responsável unicamente pela inicialização de ambiente, captura de interrupção e disparo do container IoC.
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
            AnsiConsole.MarkupLine("[bold red]\nOperação cancelada pelo usuário (Ctrl+C).[/]");
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
