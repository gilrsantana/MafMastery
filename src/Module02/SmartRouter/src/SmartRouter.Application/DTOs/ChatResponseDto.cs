namespace SmartRouter.Application.DTOs;

/// <summary>
/// Response DTO returned by the Minimal API for non-streaming completions.
/// </summary>
public record ChatResponseDto(
    string ResponseText,
    string TargetProvider,
    string Model,
    bool FallbackApplied,
    long LatencyMs,
    int? InputTokens = null,
    int? OutputTokens = null);
