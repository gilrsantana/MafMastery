namespace AiProviderBenchmarker.Domain.Model;

/// <summary>
/// Records empirical telemetry, SLA, and cost metrics for an AI model inference.
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
    /// Net generation rate (Tokens Per Second), excluding prompt preprocessing time (TTFT).
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
