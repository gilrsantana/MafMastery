namespace AiProviderBenchmarker.Domain.Model;

/// <summary>
/// Registra os dados empíricos de telemetria, SLA e custos de uma inferência de IA.
/// </summary>
public record ProviderMetric(
    string Provider,
    string ModelName,
    TimeSpan TimeToFirstToken,
    TimeSpan TotalDuration,
    int InputTokens,
    int OutputTokens,
    decimal EstimatedCostUsd,
    string GeneratedTextSnippet,
    bool Success,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Taxa de geração líquida (Tokens Por Segundo), descontando o tempo de pré-processamento de prompt (TTFT).
    /// </summary>
    public double TokensPerSecond
    {
        get
        {
            if (!Success || OutputTokens <= 0)
                return 0.0;

            var generationDurationSeconds = (TotalDuration - TimeToFirstToken).TotalSeconds;
            if (generationDurationSeconds <= 0.001)
                return OutputTokens / Math.Max(TotalDuration.TotalSeconds, 0.001);

            return Math.Round(OutputTokens / generationDurationSeconds, 2);
        }
    }
}
