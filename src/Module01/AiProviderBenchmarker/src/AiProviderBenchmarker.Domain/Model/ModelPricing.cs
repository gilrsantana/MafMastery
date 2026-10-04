namespace AiProviderBenchmarker.Domain.Model;

/// <summary>
/// Value Object encapsulating the cost per 1 Million input and output tokens.
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
