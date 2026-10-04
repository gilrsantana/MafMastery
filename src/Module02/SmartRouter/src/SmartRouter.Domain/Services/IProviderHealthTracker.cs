namespace SmartRouter.Domain.Services;

using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Models;

/// <summary>
/// Contract for tracking provider health, circuit breaker state transitions, and invocation telemetry.
/// </summary>
public interface IProviderHealthTracker
{
    /// <summary>
    /// Records a successfully completed provider invocation.
    /// </summary>
    void RecordSuccess(ProviderKind provider);

    /// <summary>
    /// Records a failed provider invocation with its exception details.
    /// </summary>
    void RecordFailure(ProviderKind provider, Exception exception);

    /// <summary>
    /// Records a transition to the open circuit state due to excessive failure rate.
    /// </summary>
    void RecordCircuitOpened(ProviderKind provider, TimeSpan breakDuration);

    /// <summary>
    /// Records a transition to the half-open state for canary trial probing.
    /// </summary>
    void RecordCircuitHalfOpened(ProviderKind provider);

    /// <summary>
    /// Records that the circuit has recovered and transitioned back to closed.
    /// </summary>
    void RecordCircuitClosed(ProviderKind provider);

    /// <summary>
    /// Retrieves the current health snapshot for the specified provider.
    /// </summary>
    ProviderHealthSnapshot GetSnapshot(ProviderKind provider);
}
