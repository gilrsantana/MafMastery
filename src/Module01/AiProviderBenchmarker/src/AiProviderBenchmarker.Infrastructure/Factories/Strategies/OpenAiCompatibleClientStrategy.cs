using System.ClientModel;
using AiProviderBenchmarker.Infrastructure.Configuration;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Strategy responsible for creating standard OpenAI clients and OpenAI /v1 API-compatible clients (Ollama, Gemini, Grok, etc.).
/// </summary>
public class OpenAiCompatibleClientStrategy : IChatClientStrategy
{
    public bool CanHandle(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        // Handles any provider adhering to OpenAI's HTTP /v1 protocol
        return true;
    }

    public IChatClient CreateClient(AiProviderConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        // If custom endpoint is present (e.g., Ollama localhost, Gemini OpenAI gateway, Grok api.x.ai, etc.)
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

        // Official default OpenAI endpoint (api.openai.com)
        var defaultClient = new OpenAIClient(config.ApiKey);
        return defaultClient.GetChatClient(config.DeploymentName).AsIChatClient();
    }
}
