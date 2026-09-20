using AiProviderBenchmarker.Infrastructure.Configuration;
using AiProviderBenchmarker.Infrastructure.Mock;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Estratégia responsável por criar clientes simulados sintéticos (offline determinístico ou fallback por falta de credenciais).
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

        // Fallback simulado para provedores reais sem credenciais
        return new SimulatedChatClient(
            modelId: $"{config.AiProviderName} (Simulado)",
            minTtftMs: config.MinTtftMs ?? 220,
            maxTtftMs: config.MaxTtftMs ?? 390,
            tokensPerSecond: config.TokensPerSecond ?? 60);
    }
}
