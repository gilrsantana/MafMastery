using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartRouter.Domain.Enums;
using SmartRouter.Infrastructure.Clients;
using SmartRouter.Infrastructure.Configuration;
using SmartRouter.Infrastructure.Health;
using SmartRouter.Infrastructure.Mock;
using SmartRouter.Infrastructure.Resilience;
using SmartRouter.Infrastructure.Strategies;

namespace SmartRouter.Tests.Resilience;

public class CircuitBreakerFallbackTests
{
    [Fact]
    public async Task CircuitBreaker_Should_Trip_After_Consecutive_Failures_And_Fallback_Instantly()
    {
        // Arrange: configuramos o circuit breaker com mínimo de 2 requisições e 50% de falha
        var options = new SmartRouterOptions
        {
            CharacterThresholdForPremiumTier = 1000,
            CircuitBreakerFailureRatio = 0.5,
            CircuitBreakerSamplingDurationSeconds = 10,
            CircuitBreakerBreakDurationSeconds = 10,
            CircuitBreakerMinimumThroughput = 2,
            TimeoutSeconds = 2,
            Providers =
            [
                new AiProviderConfig
                {
                    AiProviderName = "OpenRouter",
                    DefaultModel = "deepseek-chat",
                    Endpoint = "https://openrouter.ai/api/v1",
                    ApiKey = "fake-key",
                    PremiumTier = false
                },
                new AiProviderConfig
                {
                    AiProviderName = "AzureOpenAi",
                    DefaultModel = "gpt-4o-mini",
                    Endpoint = "https://azure.openai.com",
                    ApiKey = "fake-key",
                    PremiumTier = true
                }
            ]
        };

        var healthTracker = new ProviderHealthTracker();
        var routerStrategy = new HeuristicModelRouterStrategy(Options.Create(options));
        var resiliencePipeline = SmartRouterResilienceFactory.CreatePipeline(options, healthTracker);

        var failingPrimary = new SimulatedProviderChatClient(
            ProviderKind.EconomicProvider, 
            "deepseek-chat", 
            alwaysFail: true, 
            failureMessage: "503 Service Unavailable");

        var healthyFallback = new SimulatedProviderChatClient(
            ProviderKind.PremiumProvider, 
            "gpt-4o-mini");

        var client = new SmartRoutingChatClient(
            primaryClient: failingPrimary,
            fallbackClient: healthyFallback,
            routerStrategy: routerStrategy,
            resiliencePipeline: resiliencePipeline,
            healthTracker: healthTracker);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Teste de resiliência") };

        // Act: Faz 3 requisições consecutivas que falham no primário
        for (int i = 0; i < 3; i++)
        {
            var res = await client.GetResponseAsync(messages);
            res.AdditionalProperties!["X-SmartRouter-FallbackApplied"].Should().Be(true);
            res.AdditionalProperties!["X-SmartRouter-Target"].Should().Be(ProviderKind.PremiumProvider.ToString());
        }

        // Assert
        var snapshot = healthTracker.GetSnapshot(ProviderKind.EconomicProvider);
        snapshot.TotalFailures.Should().BeGreaterThanOrEqualTo(2);
        snapshot.IsCircuitOpen.Should().BeTrue();
        snapshot.CircuitState.Should().Be("Open");
    }
}
