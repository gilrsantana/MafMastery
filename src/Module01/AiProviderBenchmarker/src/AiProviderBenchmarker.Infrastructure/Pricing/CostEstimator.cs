using AiProviderBenchmarker.Domain.Model;
using AiProviderBenchmarker.Domain.Services;
using AiProviderBenchmarker.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AiProviderBenchmarker.Infrastructure.Pricing;

/// <summary>
/// Official inference cost estimator (FinOps) based on centralized provider settings (appsettings.json).
/// </summary>
public class CostEstimator : ICostEstimator
{
    private readonly AiProvidersOptions _options;

    public CostEstimator() : this(Options.Create(new AiProvidersOptions()))
    {
    }

    public CostEstimator(IOptions<AiProvidersOptions> options)
    {
        _options = options?.Value ?? new AiProvidersOptions();
    }

    public decimal CalculateCost(string provider, string modelName, int inputTokens, int outputTokens)
    {
        if (inputTokens <= 0 && outputTokens <= 0)
        {
            return 0m;
        }

        var pricing = GetPricing(provider, modelName);
        return pricing?.Calculate(inputTokens, outputTokens) ?? 0m;
    }

    public ModelPricing? GetPricing(string provider, string modelName)
    {
        // 1. Match by exact DeploymentName
        var config = _options.Providers.FirstOrDefault(p =>
            !string.IsNullOrWhiteSpace(p.DeploymentName) &&
            p.DeploymentName.Equals(modelName, StringComparison.OrdinalIgnoreCase));

        // 2. Lookup via GetProvider (checks model and then provider)
        config ??= _options.GetProvider(modelName);

        // 3. Lookup by provider name
        config ??= _options.GetProvider(provider);

        // 4. Fallback search by model containment
        config ??= _options.Providers.FirstOrDefault(p =>
            !string.IsNullOrWhiteSpace(p.DeploymentName) &&
            (!string.IsNullOrWhiteSpace(modelName) && modelName.Contains(p.DeploymentName, StringComparison.OrdinalIgnoreCase)));

        if (config is not null)
        {
            return new ModelPricing(
                ModelName: !string.IsNullOrWhiteSpace(modelName) ? modelName : config.DeploymentName,
                InputPricePerMillionUsd: config.InputPricePerMillion,
                OutputPricePerMillionUsd: config.OutputPricePerMillion);
        }

        // Default conservative fallback if provider/model is not registered ($0.50 / $1.50)
        return new ModelPricing(modelName, 0.50m, 1.50m);
    }
}
