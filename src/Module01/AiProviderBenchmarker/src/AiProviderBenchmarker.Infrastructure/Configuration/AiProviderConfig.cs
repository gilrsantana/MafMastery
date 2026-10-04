namespace AiProviderBenchmarker.Infrastructure.Configuration;

/// <summary>
/// Universal AI provider configuration model.
/// Supports commercial cloud providers (OpenAI, Azure, Gemini, Grok, etc.)
/// as well as local engines (Ollama, LMStudio, vLLM) and synthetic simulated engines.
/// </summary>
public class AiProviderConfig
{
    /// <summary>
    /// Provider identifier (e.g., OpenAi, AzureOpenAi, Ollama, Gemini, Grok, Simulated).
    /// </summary>
    public string AiProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Base API URL for the provider (optional for standard OpenAI, required for Azure/Ollama/Gemini/Gateways).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Authentication API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Model or deployment name (DeploymentName / ModelId).
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Composite unique identifier in the format AiProviderName:DeploymentName.
    /// </summary>
    public string Key => !string.IsNullOrWhiteSpace(DeploymentName)
        ? $"{AiProviderName}:{DeploymentName}"
        : AiProviderName;

    /// <summary>
    /// Indicates whether this provider is enabled for use.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Inference price per 1 million input tokens (FinOps).
    /// </summary>
    public decimal InputPricePerMillion { get; set; } = 0.50m;

    /// <summary>
    /// Inference price per 1 million output tokens (FinOps).
    /// </summary>
    public decimal OutputPricePerMillion { get; set; } = 1.50m;

    /// <summary>
    /// Minimum TTFT threshold in milliseconds (optional, for simulated or synthetic fallback engines).
    /// </summary>
    public int? MinTtftMs { get; set; }

    /// <summary>
    /// Maximum TTFT threshold in milliseconds (optional, for simulated or synthetic fallback engines).
    /// </summary>
    public int? MaxTtftMs { get; set; }

    /// <summary>
    /// Token generation throughput per second (optional, for simulated or synthetic fallback engines).
    /// </summary>
    public int? TokensPerSecond { get; set; }

    /// <summary>
    /// Evaluates whether the prerequisites for running inference are satisfied.
    /// </summary>
    public virtual bool IsConfigured
    {
        get
        {
            if (!Enabled || string.IsNullOrWhiteSpace(DeploymentName))
                return false;

            // Simulated engine does not depend on credentials or external networks
            if (AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase))
                return true;

            // Local endpoints (such as Ollama on localhost) do not require a mandatory ApiKey
            if (!string.IsNullOrWhiteSpace(Endpoint) &&
                (Endpoint.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                 Endpoint.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Remote providers require an ApiKey
            return !string.IsNullOrWhiteSpace(ApiKey);
        }
    }
}
