namespace SmartRouter.Domain.Enums;

/// <summary>
/// Cost and capability tiers for routing AI models.
/// </summary>
public enum RouteTier
{
    /// <summary>
    /// Low-cost, high-speed models (e.g., OpenRouter: DeepSeek Chat, Llama 3.3 70B).
    /// </summary>
    Economic,

    /// <summary>
    /// Advanced reasoning enterprise models (e.g., Azure OpenAI: GPT-4o, GPT-4o-mini).
    /// </summary>
    Premium
}
