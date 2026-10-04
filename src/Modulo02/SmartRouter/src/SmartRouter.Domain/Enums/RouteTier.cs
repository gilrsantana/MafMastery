namespace SmartRouter.Domain.Enums;

/// <summary>
/// Níveis de custo e capacidade para roteamento de modelos de IA.
/// </summary>
public enum RouteTier
{
    /// <summary>
    /// Modelos de baixo custo e alta velocidade (ex.: OpenRouter: DeepSeek Chat, Llama 3.3 70B).
    /// </summary>
    Economic,

    /// <summary>
    /// Modelos avançados de raciocínio corporativo (ex.: Azure OpenAI: GPT-4o, GPT-4o-mini).
    /// </summary>
    Premium
}
