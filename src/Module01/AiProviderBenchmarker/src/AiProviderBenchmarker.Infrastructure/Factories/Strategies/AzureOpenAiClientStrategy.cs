using System.ClientModel;
using AiProviderBenchmarker.Infrastructure.Configuration;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Estratégia responsável por criar clientes especializados para o Azure OpenAI Service.
/// </summary>
public class AzureOpenAiClientStrategy : IChatClientStrategy
{
    public bool CanHandle(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return config.AiProviderName.Equals("AzureOpenAi", StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrWhiteSpace(config.Endpoint) && config.Endpoint.Contains("openai.azure.com", StringComparison.OrdinalIgnoreCase));
    }

    public IChatClient CreateClient(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var azureClient = new AzureOpenAIClient(
            new Uri(config.Endpoint),
            new ApiKeyCredential(config.ApiKey));

        return azureClient.GetChatClient(config.DeploymentName).AsIChatClient();
    }
}
