namespace SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Configurações consolidadas do SmartRouter Gateway.
/// </summary>
public record SmartRouterOptions
{
    public const string SectionName = "SmartRouter";

    public List<AiProviderConfig> Providers { get; set; } = [];

    /// <summary>
    /// Valida se existem exatamente dois provedores configurados, sendo um PremiumTier e o outro não.
    /// </summary>
    public bool HasValidProviders() =>
        Providers is { Count: 2 } && Providers.Count(p => p.PremiumTier) == 1;
    
    /// <summary>
    /// Valida se Provides tem configuração mínima para operação
    /// </summary>
    /// <returns>bool</returns>
    public bool IsConfigured()
        => Providers.Count == 2
            && Providers.All(x => !string.IsNullOrEmpty(x.ApiKey))
            && Providers.All(x => !string.IsNullOrEmpty(x.Endpoint));

    public AiProviderConfig GetEconomicAiProvider()
        => Providers.Single(x => !x.PremiumTier);
    public AiProviderConfig GetPremiumAiProvider()
        => Providers.Single(x => x.PremiumTier);

    /// <summary>
    /// Limiar de caracteres no prompt para encaminhar diretamente à rota Premium (default: 1000).
    /// </summary>
    public int CharacterThresholdForPremiumTier { get; init; } = 1000;

    /// <summary>
    /// Taxa de falhas (0.0 a 1.0) para abertura do Circuit Breaker (default: 0.5 = 50%).
    /// </summary>
    public double CircuitBreakerFailureRatio { get; init; } = 0.5;

    /// <summary>
    /// Janela amostral em segundos para contagem de falhas (default: 30s).
    /// </summary>
    public int CircuitBreakerSamplingDurationSeconds { get; init; } = 30;

    /// <summary>
    /// Tempo de abertura do circuito (resfriamento) em segundos (default: 30s).
    /// </summary>
    public int CircuitBreakerBreakDurationSeconds { get; init; } = 30;

    /// <summary>
    /// Quantidade mínima de requisições na janela amostral para avaliar abertura do circuito (default: 5).
    /// </summary>
    public int CircuitBreakerMinimumThroughput { get; init; } = 5;

    /// <summary>
    /// Timeout máximo para resposta do provedor primário antes de acionar fallback (default: 15s).
    /// </summary>
    public int TimeoutSeconds { get; init; } = 15;

    /// <summary>
    /// Se true, utiliza clientes simulados quando as chaves de API não estiverem configuradas no ambiente.
    /// </summary>
    public bool UseSimulatedClientsIfUnconfigured { get; init; } = true;
}
