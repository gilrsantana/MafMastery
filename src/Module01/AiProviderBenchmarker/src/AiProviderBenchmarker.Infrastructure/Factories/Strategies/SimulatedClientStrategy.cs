using AiProviderBenchmarker.Infrastructure.Configuration;
using AiProviderBenchmarker.Infrastructure.Mock;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Strategy responsible for creating synthetic simulated clients (deterministic offline mock or missing-credentials fallback).
/// </summary>
public class SimulatedClientStrategy : IChatClientStrategy
{
    public bool CanHandle(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return config.AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase) || !config.IsConfigured;
    }

    public IChatClient CreateClient(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (config.AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase))
        {
            var model = !string.IsNullOrWhiteSpace(config.DeploymentName)
                ? config.DeploymentName
                : "simulated-fast-llm";

            return new SimulatedChatClient(
                modelId: model,
                minTtftMs: config.MinTtftMs ?? 160,
                maxTtftMs: config.MaxTtftMs ?? 350,
                tokensPerSecond: config.TokensPerSecond ?? 50);
        }

        // Simulated fallback for real providers without configured credentials
        return new SimulatedChatClient(
            modelId: $"{config.AiProviderName} (Simulated)",
            minTtftMs: config.MinTtftMs ?? 220,
            maxTtftMs: config.MaxTtftMs ?? 390,
            tokensPerSecond: config.TokensPerSecond ?? 60);
    }
}
