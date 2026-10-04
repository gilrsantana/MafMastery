namespace SmartRouter.Infrastructure.Health;

using System.Collections.Concurrent;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Models;
using SmartRouter.Domain.Services;

/// <summary>
/// Thread-safe tracker for health metrics and Circuit Breaker state transitions.
/// </summary>
public class ProviderHealthTracker : IProviderHealthTracker
{
    private class HealthState
    {
        public string CircuitState { get; set; } = "Closed";
        public bool IsCircuitOpen { get; set; }
        public int ConsecutiveFailures { get; set; }
        public long TotalSuccesses { get; set; }
        public long TotalFailures { get; set; }
        public string? LastFailureReason { get; set; }
        public DateTimeOffset LastCheckedUtc { get; set; } = DateTimeOffset.UtcNow;
    }

    private readonly ConcurrentDictionary<ProviderKind, HealthState> _states = new();
    private readonly object _lock = new();

    public void RecordSuccess(ProviderKind provider)
    {
        var state = GetOrCreate(provider);
        lock (_lock)
        {
            state.TotalSuccesses++;
            state.ConsecutiveFailures = 0;
            state.LastCheckedUtc = DateTimeOffset.UtcNow;
        }
    }

    public void RecordFailure(ProviderKind provider, Exception exception)
    {
        var state = GetOrCreate(provider);
        lock (_lock)
        {
            state.TotalFailures++;
            state.ConsecutiveFailures++;
            state.LastFailureReason = exception.Message;
            state.LastCheckedUtc = DateTimeOffset.UtcNow;
        }
    }

    public void RecordCircuitOpened(ProviderKind provider, TimeSpan breakDuration)
    {
        var state = GetOrCreate(provider);
        lock (_lock)
        {
            state.CircuitState = "Open";
            state.IsCircuitOpen = true;
            state.LastCheckedUtc = DateTimeOffset.UtcNow;
        }
    }

    public void RecordCircuitHalfOpened(ProviderKind provider)
    {
        var state = GetOrCreate(provider);
        lock (_lock)
        {
            state.CircuitState = "HalfOpen";
            state.IsCircuitOpen = false;
            state.LastCheckedUtc = DateTimeOffset.UtcNow;
        }
    }

    public void RecordCircuitClosed(ProviderKind provider)
    {
        var state = GetOrCreate(provider);
        lock (_lock)
        {
            state.CircuitState = "Closed";
            state.IsCircuitOpen = false;
            state.ConsecutiveFailures = 0;
            state.LastCheckedUtc = DateTimeOffset.UtcNow;
        }
    }

    public ProviderHealthSnapshot GetSnapshot(ProviderKind provider)
    {
        var state = GetOrCreate(provider);
        lock (_lock)
        {
            return new ProviderHealthSnapshot(
                Provider: provider,
                CircuitState: state.CircuitState,
                IsCircuitOpen: state.IsCircuitOpen,
                ConsecutiveFailures: state.ConsecutiveFailures,
                TotalSuccesses: state.TotalSuccesses,
                TotalFailures: state.TotalFailures,
                LastFailureReason: state.LastFailureReason,
                LastCheckedUtc: state.LastCheckedUtc);
        }
    }

    private HealthState GetOrCreate(ProviderKind provider)
    {
        return _states.GetOrAdd(provider, _ => new HealthState());
    }
}
