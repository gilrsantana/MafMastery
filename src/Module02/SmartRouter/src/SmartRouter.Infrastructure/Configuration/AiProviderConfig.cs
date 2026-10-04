namespace SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Modelo universal de configuração de provedor de IA.
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
    public string DefaultModel { get; set; } = string.Empty;
    
    /// <summary>
    /// Nome da aplicação base.
    /// </summary>
    public string AppTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// Endereço http base da aplicação.
    /// </summary>
    public string HttpReferer { get; set; } = string.Empty;
    
    /// <summary>
    /// Identificação do Tier de Operação ("Economy" ou "Premium").
    /// </summary>
    public bool PremiumTier { get; set; }

    /// <summary>
    /// Valida se o provedor possui os campos mínimos necessários (Endpoint e ApiKey).
    /// </summary>
    public bool IsConfigured() => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(Endpoint);
}