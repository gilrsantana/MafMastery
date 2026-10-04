namespace SmartRouter.Application.DTOs;

/// <summary>
/// Telemetry DTO exposed by the /health/circuit diagnostic endpoint.
/// </summary>
public record CircuitStatusDto(
    string Provider,
    string CircuitState,
    bool IsOpen,
    int ConsecutiveFailures,
    long TotalSuccesses,
    long TotalFailures,
    string? LastFailureReason,
    DateTimeOffset LastEvaluatedUtc);
