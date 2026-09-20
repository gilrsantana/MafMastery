using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Application.Common;

/// <summary>
/// Fábrica desacoplada para instanciação de clientes que implementam IChatClient.
/// </summary>
public interface IChatClientFactory
{
    IChatClient CreateClient(string providerName);
    string GetModelName(string providerName);
    string GetProviderName(string providerName) => providerName;
    IReadOnlyList<string> GetConfiguredProviders();
}
