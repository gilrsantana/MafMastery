namespace AiProviderBenchmarker.Cli.Arguments;

/// <summary>
/// Representa as opções processadas passadas para a CLI.
/// </summary>
public record CliParsedOptions(
    bool IsHelp,
    bool IsNonInteractive,
    string Prompt,
    IReadOnlyList<string> Providers,
    int MaxTokens);
