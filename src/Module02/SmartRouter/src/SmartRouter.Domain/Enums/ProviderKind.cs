namespace SmartRouter.Domain.Enums;

/// <summary>
/// AI inference provider types supported by the gateway.
/// </summary>
public enum ProviderKind
{
    /// <summary>
    /// Primary provider via OpenRouter aggregator or local economy model.
    /// </summary>
    EconomicProvider,

    /// <summary>
    /// Secondary enterprise provider hosted in private cloud (e.g., Azure OpenAI / Gemini).
    /// </summary>
    PremiumProvider,

    /// <summary>
    /// Contingency route triggered upon Circuit Breaker opening.
    /// </summary>
    FallbackCircuit
}
