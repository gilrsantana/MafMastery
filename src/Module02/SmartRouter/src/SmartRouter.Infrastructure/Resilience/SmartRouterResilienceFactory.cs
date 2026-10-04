namespace SmartRouter.Infrastructure.Resilience;

using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Services;
using SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Fábrica de pipelines de resiliência baseada no Polly v8 com Zero Allocation.
/// </summary>
public static class SmartRouterResilienceFactory
{
    /// <summary>
    /// Cria o pipeline de resiliência combinando Timeout e Circuit Breaker para o provedor primário.
    /// </summary>
    public static ResiliencePipeline CreatePipeline(
        SmartRouterOptions options, 
        IProviderHealthTracker healthTracker)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(healthTracker);

        return new ResiliencePipelineBuilder()
            // 1. Timeout Estratégico para SLA de IA
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds),
                OnTimeout = _ => default
            })
            // 2. Circuit Breaker reativo com callbacks no IProviderHealthTracker
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
