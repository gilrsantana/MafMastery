namespace SmartRouter.Application.DTOs;

/// <summary>
/// DTO da resposta retornada pela Minimal API para invocações não-streaming.
/// </summary>
public record ChatResponseDto(
    string ResponseText,
    string TargetProvider,
    string Model,
    bool FallbackApplied,
    long LatencyMs,
    int? InputTokens = null,
    int? OutputTokens = null);
