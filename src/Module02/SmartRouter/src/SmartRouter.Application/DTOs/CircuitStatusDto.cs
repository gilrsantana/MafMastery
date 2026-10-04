namespace SmartRouter.Application.DTOs;

/// <summary>
/// DTO exposto pelo endpoint de telemetria diagnóstica /health/circuit.
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
