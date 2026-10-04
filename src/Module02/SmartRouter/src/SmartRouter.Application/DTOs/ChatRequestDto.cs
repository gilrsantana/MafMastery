namespace SmartRouter.Application.DTOs;

/// <summary>
/// Chat request DTO received by Minimal API endpoints.
/// </summary>
public record ChatRequestDto
{
    /// <summary>
    /// Conversation message history.
    /// </summary>
    public List<ChatMessageDto> Messages { get; init; } = [];

    /// <summary>
    /// Explicit model requested by caller (optional; if omitted, the router determines automatically).
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// Temperature hyperparameter for stochastic sampling (optional).
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// Maximum output token limit for generated completion (optional).
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Force a route tier preference: "Economy" or "Premium" (optional).
    /// </summary>
    public string? RouteTierPreference { get; init; }
}
