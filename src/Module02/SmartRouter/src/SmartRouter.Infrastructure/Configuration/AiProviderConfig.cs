namespace SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Universal AI provider configuration model.
/// </summary>
public class AiProviderConfig
{
    /// <summary>
    /// Provider identifier (e.g., OpenAi, AzureOpenAi, Ollama, Gemini, Grok, Simulated).
    /// </summary>
    public string AiProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Base API URL of the provider (optional for standard OpenAI, required for Azure/Ollama/Gemini/gateways).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Authentication API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model or deployment identifier (DeploymentName / ModelId).
    /// </summary>
    public string DefaultModel { get; set; } = string.Empty;
    
    /// <summary>
    /// Base application name (e.g., for OpenRouter identification headers).
    /// </summary>
    public string AppTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// Base application HTTP referrer URL.
    /// </summary>
    public string HttpReferer { get; set; } = string.Empty;
    
    /// <summary>
    /// Operational tier flag (true for Premium, false for Economic).
    /// </summary>
    public bool PremiumTier { get; set; }

    /// <summary>
    /// Validates whether the provider has minimum required configuration (Endpoint and ApiKey).
    /// </summary>
    public bool IsConfigured() => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Endpoint);
}