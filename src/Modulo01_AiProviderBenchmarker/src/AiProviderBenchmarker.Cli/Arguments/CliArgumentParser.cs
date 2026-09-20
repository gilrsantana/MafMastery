using AiProviderBenchmarker.Domain.Model;
using Spectre.Console;

namespace AiProviderBenchmarker.Cli.Arguments;

/// <summary>
/// Analisador de argumentos de linha de comando com suporte a execução interativa e não-interativa.
/// </summary>
public class CliArgumentParser
{
    public const string DefaultPrompt = 
        "Escreva uma função em C# 14 (.NET 10) que calcula Fibonacci usando busca binária em uma sequência pré-computada e trate exceções.";

    public const int DefaultMaxTokens = 300;

    public CliParsedOptions Parse(string[] args)
    {
        var isHelp = args.Contains("--help", StringComparer.OrdinalIgnoreCase) || 
                     args.Contains("-h", StringComparer.OrdinalIgnoreCase);

        var prompt = GetArgValue(args, "--prompt", "-p") ?? DefaultPrompt;

        var maxTokens = DefaultMaxTokens;
        var maxTokensArg = GetArgValue(args, "--max-tokens", "-m");
        if (int.TryParse(maxTokensArg, out var parsedTokens) && parsedTokens > 0)
        {
            maxTokens = parsedTokens;
        }

        var providers = ParseProvidersArg(GetArgValue(args, "--providers", "--provider"));

        var isNonInteractive = !AnsiConsole.Profile.Capabilities.Interactive || args.Length > 0;

        return new CliParsedOptions(
            IsHelp: isHelp,
            IsNonInteractive: isNonInteractive,
            Prompt: prompt,
            Providers: providers,
            MaxTokens: maxTokens);
    }

    private static string? GetArgValue(string[] args, string flag, string? shortFlag = null)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(flag, StringComparison.OrdinalIgnoreCase) ||
                (shortFlag != null && args[i].Equals(shortFlag, StringComparison.OrdinalIgnoreCase)))
            {
                if (i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
        }
        return null;
    }

    private static List<string> ParseProvidersArg(string? providersArg)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(providersArg))
            return list;

        var parts = providersArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        list.AddRange(parts);
        return list;
    }
}
