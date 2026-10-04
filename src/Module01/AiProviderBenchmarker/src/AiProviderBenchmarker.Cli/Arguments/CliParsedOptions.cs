namespace AiProviderBenchmarker.Cli.Arguments;

/// <summary>
/// Represents parsed command-line arguments passed to the CLI.
/// </summary>
public record CliParsedOptions(
    bool IsHelp,
    bool IsNonInteractive,
    string Prompt,
    IReadOnlyList<string> Providers,
    int MaxTokens);
