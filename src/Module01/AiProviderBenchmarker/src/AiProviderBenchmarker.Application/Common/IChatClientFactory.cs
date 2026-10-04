using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Application.Common;

/// <summary>
/// Decoupled factory for instantiating clients implementing IChatClient.
/// </summary>
public interface IChatClientFactory
{
    IChatClient CreateClient(string providerName);
    string GetModelName(string providerName);
    string GetProviderName(string providerName) => providerName;
    IReadOnlyList<string> GetConfiguredProviders();
}
