namespace AiProviderBenchmarker.Domain.Model;

/// <summary>
/// Value Object que encapsula o custo por 1 Milhão de tokens de entrada e saída.
/// </summary>
public record ModelPricing(
    string ModelName,
    decimal InputPricePerMillionUsd,
    decimal OutputPricePerMillionUsd)
{
    public decimal Calculate(int inputTokens, int outputTokens)
    {
        var inputCost = (inputTokens / 1_000_000m) * InputPricePerMillionUsd;
        var outputCost = (outputTokens / 1_000_000m) * OutputPricePerMillionUsd;
        return decimal.Round(inputCost + outputCost, 6);
    }
}
