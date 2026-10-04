namespace SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Consolidated configuration options for SmartRouter Gateway.
/// </summary>
public record SmartRouterOptions
{
    public const string SectionName = "SmartRouter";

    public List<AiProviderConfig> Providers { get; set; } = [];

    /// <summary>
    /// Validates that exactly two providers are configured, one with PremiumTier and the other without.
    /// </summary>
    public bool HasValidProviders() =>
        Providers is { Count: 2 } && Providers.Count(p => p.PremiumTier) == 1;
    
    /// <summary>
    /// Validates whether configured providers meet minimal operational requirements.
    /// </summary>
    public bool IsConfigured()
        => Providers.Count == 2
            && Providers.All(x => !string.IsNullOrEmpty(x.ApiKey))
            && Providers.All(x => !string.IsNullOrEmpty(x.Endpoint));

    public AiProviderConfig GetEconomicAiProvider()
        => Providers.Single(x => !x.PremiumTier);
    public AiProviderConfig GetPremiumAiProvider()
        => Providers.Single(x => x.PremiumTier);

    /// <summary>
    /// Character count threshold to route prompt directly to Premium tier (default: 1000).
    /// </summary>
    public int CharacterThresholdForPremiumTier { get; init; } = 1000;

    /// <summary>
    /// Failure ratio (0.0 to 1.0) triggering Circuit Breaker opening (default: 0.5 = 50%).
    /// </summary>
    public double CircuitBreakerFailureRatio { get; init; } = 0.5;

    /// <summary>
    /// Sampling duration window in seconds for failure rate evaluation (default: 30s).
    /// </summary>
    public int CircuitBreakerSamplingDurationSeconds { get; init; } = 30;

    /// <summary>
    /// Circuit break duration (cooldown period) in seconds (default: 30s).
    /// </summary>
    public int CircuitBreakerBreakDurationSeconds { get; init; } = 30;

    /// <summary>
    /// Minimum throughput count in sampling window required before evaluating circuit breaker (default: 5).
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; init; } = 5;

    /// <summary>
    /// Maximum response timeout for the primary provider before failing over (default: 15s).
    /// </summary>
    public int TimeoutSeconds { get; init; } = 15;

    /// <summary>
    /// If true, uses high-fidelity simulated clients when API keys are unconfigured.
    /// </summary>
    public bool UseSimulatedClientsIfUnconfigured { get; init; } = true;
}
