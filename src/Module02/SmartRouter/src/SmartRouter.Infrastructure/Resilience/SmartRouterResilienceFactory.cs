namespace SmartRouter.Infrastructure.Resilience;

using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Services;
using SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Resilience pipeline factory based on zero-allocation Polly v8.
/// </summary>
public static class SmartRouterResilienceFactory
{
    /// <summary>
    /// Creates the resilience pipeline combining Timeout and Circuit Breaker for the primary provider.
    /// </summary>
    public static ResiliencePipeline CreatePipeline(
        SmartRouterOptions options, 
        IProviderHealthTracker healthTracker)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(healthTracker);

        return new ResiliencePipelineBuilder()
            // 1. Strategic Timeout for AI SLA enforcement
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds),
                OnTimeout = _ => default
            })
            // 2. Reactive Circuit Breaker with telemetry callbacks to IProviderHealthTracker
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = options.CircuitBreakerFailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerSamplingDurationSeconds),
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds),
                MinimumThroughput = options.CircuitBreakerMinimumThroughput,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnOpened = args =>
                {
                    healthTracker.RecordCircuitOpened(ProviderKind.EconomicProvider, args.BreakDuration);
                    return default;
                },
                OnClosed = _ =>
                {
                    healthTracker.RecordCircuitClosed(ProviderKind.EconomicProvider);
                    return default;
                },
                OnHalfOpened = _ =>
                {
                    healthTracker.RecordCircuitHalfOpened(ProviderKind.EconomicProvider);
                    return default;
                }
            })
            .Build();
    }
}
