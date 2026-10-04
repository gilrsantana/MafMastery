namespace SmartRouter.Application.DTOs;

public record ProviderHealthItemDto(
    string Provider,
    string Tier,
    string CircuitState,
    bool IsCircuitOpen,
    int ConsecutiveFailures,
    long TotalSuccesses,
    long TotalFailures,
    string? LastFailureReason,
    DateTimeOffset LastEvaluatedUtc,
    long? ProbeLatencyMs = null,
    bool? IsAlive = null);