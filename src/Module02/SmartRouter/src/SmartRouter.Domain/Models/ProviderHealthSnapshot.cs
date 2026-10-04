namespace SmartRouter.Domain.Models;

using SmartRouter.Domain.Enums;

/// <summary>
/// Health state and telemetry snapshot of a provider monitored by Circuit Breaker.
/// </summary>
/// <param name="Provider">Evaluated provider.</param>
/// <param name="CircuitState">Current circuit state name (Closed, Open, HalfOpen).</param>
/// <param name="IsCircuitOpen">Indicates whether the circuit is open (blocking direct calls).</param>
/// <param name="ConsecutiveFailures">Consecutive failure count accumulated in the current sampling window.</param>
/// <param name="TotalSuccesses">Total requests successfully completed.</param>
/// <param name="TotalFailures">Total requests that failed.</param>
/// <param name="LastFailureReason">Error message of the last intercepted exception.</param>
/// <param name="LastCheckedUtc">UTC timestamp of the latest health inspection or state change.</param>
public record ProviderHealthSnapshot(
    ProviderKind Provider,
    string CircuitState,
    bool IsCircuitOpen,
    int ConsecutiveFailures,
    long TotalSuccesses,
    long TotalFailures,
    string? LastFailureReason,
    DateTimeOffset LastCheckedUtc);
