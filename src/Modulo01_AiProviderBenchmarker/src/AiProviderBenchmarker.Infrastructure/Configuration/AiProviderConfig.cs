namespace AiProviderBenchmarker.Infrastructure.Configuration;

/// <summary>
/// Modelo universal de configuração de provedor de IA.
/// Atende tanto provedores comerciais em nuvem (OpenAI, Azure, Gemini, Grok, etc.)
/// quanto motores locais (Ollama, LMStudio, vLLM) e motores simulados sintéticos.
/// </summary>
public class AiProviderConfig
{
    /// <summary>
    /// Identificador do provedor (ex: OpenAi, AzureOpenAi, Ollama, Gemini, Grok, Simulated).
    /// </summary>
    public string AiProviderName { get; set; } = string.Empty;

    /// <summary>
    /// URL base da API do provedor (opcional na OpenAI padrão, obrigatória em Azure/Ollama/Gemini/Gateways).
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Chave de API de autenticação.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Nome do modelo ou implantação (DeploymentName / ModelId).
    /// </summary>
    public string DeploymentName { get; set; } = string.Empty;

    /// <summary>
    /// Chave composta identificadora única no formato AiProviderName:DeploymentName.
    /// </summary>
    public string Key => !string.IsNullOrWhiteSpace(DeploymentName)
        ? $"{AiProviderName}:{DeploymentName}"
        : AiProviderName;

    /// <summary>
    /// Indica se este provedor está habilitado para uso.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Preço de inferência por 1 milhão de tokens de entrada (FinOps).
    /// </summary>
    public decimal InputPricePerMillion { get; set; } = 0.50m;

    /// <summary>
    /// Preço de inferência por 1 milhão de tokens de saída (FinOps).
    /// </summary>
    public decimal OutputPricePerMillion { get; set; } = 1.50m;

    /// <summary>
    /// Limite mínimo de TTFT em milissegundos (opcional, exclusivo para motores simulados ou fallback sintético).
    /// </summary>
    public int? MinTtftMs { get; set; }

    /// <summary>
    /// Limite máximo de TTFT em milissegundos (opcional, exclusivo para motores simulados ou fallback sintético).
    /// </summary>
    public int? MaxTtftMs { get; set; }

    /// <summary>
    /// Vazão de tokens por segundo (opcional, exclusivo para motores simulados ou fallback sintético).
    /// </summary>
    public int? TokensPerSecond { get; set; }

    /// <summary>
    /// Avalia se os pré-requisitos para execução de inferência foram atendidos.
    /// </summary>
    public virtual bool IsConfigured
    {
        get
        {
            if (!Enabled || string.IsNullOrWhiteSpace(DeploymentName))
                return false;

            // O motor simulado não depende de credenciais nem de rede externa
            if (AiProviderName.Equals("Simulated", StringComparison.OrdinalIgnoreCase))
                return true;

            // Endpoints locais (como Ollama em localhost) não exigem ApiKey obrigatória
            if (!string.IsNullOrWhiteSpace(Endpoint) &&
                (Endpoint.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                 Endpoint.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Provedores remotos exigem ApiKey
            return !string.IsNullOrWhiteSpace(ApiKey);
        }
    }
}
