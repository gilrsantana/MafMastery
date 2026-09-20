using AiProviderBenchmarker.Domain.Model;

namespace AiProviderBenchmarker.Domain.Services;

/// <summary>
/// Contrato de serviço de domínio para estimativa de custos de inferência (FinOps).
/// </summary>
public interface ICostEstimator
{
    decimal CalculateCost(string provider, string modelName, int inputTokens, int outputTokens);
    ModelPricing? GetPricing(string provider, string modelName);
}
