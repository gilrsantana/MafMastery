using AiProviderBenchmarker.Domain.Model;

namespace AiProviderBenchmarker.Domain.Services;

/// <summary>
/// Domain service contract for inference cost estimation (FinOps).
/// </summary>
public interface ICostEstimator
{
    decimal CalculateCost(string provider, string modelName, int inputTokens, int outputTokens);
    ModelPricing? GetPricing(string provider, string modelName);
}
