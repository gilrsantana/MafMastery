namespace SmartRouter.Domain.Enums;

/// <summary>
/// Tipos de provedores de inferência de IA homologados no gateway.
/// </summary>
public enum ProviderKind
{
    /// <summary>
    /// Provedor primário via agregador OpenRouter.
    /// </summary>
    EconomicProvider,

    /// <summary>
    /// Provedor secundário corporativo em nuvem privada Azure OpenAI.
    /// </summary>
    PremiumProvider,

    /// <summary>
    /// Rota de contingência acionada por abertura de Circuit Breaker.
    /// </summary>
    FallbackCircuit
}
