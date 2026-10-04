using System.ClientModel;
using AiProviderBenchmarker.Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Estratégia responsável por criar clientes OpenAI padrão e compatíveis com a API OpenAI /v1 (Ollama, Gemini, Grok, etc.).
/// </summary>
public class OpenAiCompatibleClientStrategy : IChatClientStrategy
{
    public bool CanHandle(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        // Atende qualquer provedor que utilize o protocolo HTTP /v1 da OpenAI
        return true;
    }

    public IChatClient CreateClient(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        // Se possuir endpoint customizado (ex: Ollama localhost, Gemini OpenAI gateway, Grok api.x.ai, etc.)
        if (!string.IsNullOrWhiteSpace(config.Endpoint))
        {
            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(config.Endpoint)
            };
            var key = string.IsNullOrWhiteSpace(config.ApiKey) ? "ollama-or-local" : config.ApiKey;
            var customClient = new OpenAIClient(new ApiKeyCredential(key), clientOptions);
            return customClient.GetChatClient(config.DeploymentName).AsIChatClient();
        }

        // Endpoint padrão oficial da OpenAI (api.openai.com)
        var defaultClient = new OpenAIClient(config.ApiKey);
        return defaultClient.GetChatClient(config.DeploymentName).AsIChatClient();
    }
}
