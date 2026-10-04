using AiProviderBenchmarker.Infrastructure.Configuration;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Strategy contract for creating specialized IChatClient adapters based on AI provider type or protocol.
/// </summary>
public interface IChatClientStrategy
{
    /// <summary>
    /// Evaluates whether the strategy can handle the given provider configuration.
    /// </summary>
    bool CanHandle(AiProviderConfig config);

    /// <summary>
    /// Creates the configured IChatClient instance.
    /// </summary>
    IChatClient CreateClient(AiProviderConfig config);
}
