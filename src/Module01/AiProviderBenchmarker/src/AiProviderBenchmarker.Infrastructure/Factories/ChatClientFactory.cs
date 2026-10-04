using AiProviderBenchmarker.Application.Common;
using AiProviderBenchmarker.Infrastructure.Configuration;
using AiProviderBenchmarker.Infrastructure.Factories.Strategies;
using AiProviderBenchmarker.Infrastructure.Mock;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace AiProviderBenchmarker.Infrastructure.Factories;

/// <summary>
/// Fábrica desacoplada e dinâmica de adaptadores IChatClient baseada no Padrão Strategy.
/// </summary>
public class ChatClientFactory : IChatClientFactory
{
    private readonly IReadOnlyList<IChatClientStrategy> _strategies;
    private readonly AiProvidersOptions _options;

    public ChatClientFactory(IOptions<AiProvidersOptions> options)
        : this(GetDefaultStrategies(), options)
    {
    }

    public ChatClientFactory(
        IEnumerable<IChatClientStrategy> strategies,
        IOptions<AiProvidersOptions> options)
    {
        _strategies = strategies?.ToList() ?? GetDefaultStrategies();
        _options = options?.Value ?? new AiProvidersOptions();
    }

    public IChatClient CreateClient(string providerName)
    {
        var config = _options.GetProvider(providerName);
        if (config == null)
        {
            return new SimulatedChatClient($"{providerName} (Simulado)", minTtftMs: 200, maxTtftMs: 400, tokensPerSecond: 50);
        }

        return CreateClient(config);
    }

    public IChatClient CreateClient(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(config))
            ?? throw new NotSupportedException($"Nenhuma estratégia compatível encontrada para o provedor '{config.AiProviderName}'.");

        return strategy.CreateClient(config);
    }

    private static List<IChatClientStrategy> GetDefaultStrategies() =>
    [
        new SimulatedClientStrategy(),
        new AzureOpenAiClientStrategy(),
        new OpenAiCompatibleClientStrategy()
    ];

    public string GetProviderName(string providerName)
    {
        var config = _options.GetProvider(providerName);
        return config?.AiProviderName ?? providerName;
    }

    public string GetModelName(string providerName)
    {
        var config = _options.GetProvider(providerName);
        if (config == null) 
            return $"{providerName} (Simulado)";

        if (config.AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrWhiteSpace(config.DeploymentName) 
                ? config.DeploymentName 
                : "simulated-fast-llm";
        }

        return config.IsConfigured ? config.DeploymentName : $"{providerName} (Simulado)";
    }

    public IReadOnlyList<string> GetConfiguredProviders()
    {
        var providers = new List<string>();

        foreach (var p in _options.Providers)
        {
            if (p.AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase)) continue;

            if (p.IsConfigured)
            {
                providers.Add(p.Key);
            }
        }

        var sim = _options.GetProvider("Simulated");
        if (providers.Count == 0 || (sim?.Enabled ?? true))
        {
            var simKey = sim != null ? sim.Key : "Simulated";
            providers.Add(simKey);
        }

        return providers;
    }
}
