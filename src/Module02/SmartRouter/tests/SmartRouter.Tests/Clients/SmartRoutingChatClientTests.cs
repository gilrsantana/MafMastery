using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Polly;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Services;
using SmartRouter.Domain.Strategies;
using SmartRouter.Infrastructure.Clients;
using SmartRouter.Infrastructure.Configuration;
using SmartRouter.Infrastructure.Health;
using SmartRouter.Infrastructure.Mock;
using SmartRouter.Infrastructure.Resilience;
using SmartRouter.Infrastructure.Strategies;

namespace SmartRouter.Tests.Clients;

public class SmartRoutingChatClientTests
{
    private readonly IModelRouterStrategy _routerStrategy;
    private readonly IProviderHealthTracker _healthTracker;
    private readonly SmartRouterOptions _options;
    private readonly ResiliencePipeline _resiliencePipeline;

    public SmartRoutingChatClientTests()
    {
        _options = new SmartRouterOptions
        {
            CharacterThresholdForPremiumTier = 1000,
            CircuitBreakerFailureRatio = 0.5,
            CircuitBreakerSamplingDurationSeconds = 30,
            CircuitBreakerBreakDurationSeconds = 30,
            CircuitBreakerMinimumThroughput = 5,
            TimeoutSeconds = 4,
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

        _routerStrategy = new HeuristicModelRouterStrategy(Options.Create(_options));
        _healthTracker = new ProviderHealthTracker();
        _resiliencePipeline = SmartRouterResilienceFactory.CreatePipeline(_options, _healthTracker);
    }

    [Fact]
    public async Task GetResponseAsync_Should_Use_Primary_When_Available_And_Route_Is_Economy()
    {
        // Arrange
        var primaryMock = new SimulatedProviderChatClient(ProviderKind.EconomicProvider, "deepseek-chat");
        var fallbackMock = new SimulatedProviderChatClient(ProviderKind.PremiumProvider, "gpt-4o-mini");

        var client = new SmartRoutingChatClient(
            primaryClient: primaryMock,
            fallbackClient: fallbackMock,
            routerStrategy: _routerStrategy,
            resiliencePipeline: _resiliencePipeline,
            healthTracker: _healthTracker);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Hello, world!") };

        // Act
        var response = await client.GetResponseAsync(messages);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().Contain("Simulated OpenRouter");
        response.AdditionalProperties!["X-SmartRouter-Target"].Should().Be(ProviderKind.EconomicProvider.ToString());
        response.AdditionalProperties!["X-SmartRouter-FallbackApplied"].Should().Be(false);
    }

    [Fact]
    public async Task GetResponseAsync_Should_Fallback_To_Azure_When_Primary_Fails()
    {
        // Arrange (Primary configured to always fail)
        var primaryFailing = new SimulatedProviderChatClient(
            ProviderKind.EconomicProvider, 
            "deepseek-chat", 
            alwaysFail: true, 
            failureMessage: "OpenRouter rate limit or timeout");
        var fallbackMock = new SimulatedProviderChatClient(ProviderKind.PremiumProvider, "gpt-4o-mini");

        var client = new SmartRoutingChatClient(
            primaryClient: primaryFailing,
            fallbackClient: fallbackMock,
            routerStrategy: _routerStrategy,
            resiliencePipeline: _resiliencePipeline,
            healthTracker: _healthTracker);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Prompt that will fail on primary") };

        // Act
        var response = await client.GetResponseAsync(messages);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().Contain("Simulated AzureOpenAi");
        response.AdditionalProperties!["X-SmartRouter-Target"].Should().Be(ProviderKind.PremiumProvider.ToString());
        response.AdditionalProperties!["X-SmartRouter-FallbackApplied"].Should().Be(true);
    }

    [Fact]
    public async Task GetResponseAsync_Should_Route_Directly_To_Azure_When_Route_Is_Premium()
    {
        // Arrange
        var primaryMock = new SimulatedProviderChatClient(ProviderKind.EconomicProvider, "deepseek-chat");
        var fallbackMock = new SimulatedProviderChatClient(ProviderKind.PremiumProvider, "gpt-4o-mini");

        var client = new SmartRoutingChatClient(
            primaryClient: primaryMock,
            fallbackClient: fallbackMock,
            routerStrategy: _routerStrategy,
            resiliencePipeline: _resiliencePipeline,
            healthTracker: _healthTracker);

        // Long prompt triggers Premium
        var longPrompt = new string('x', 1200);
        var messages = new List<ChatMessage> { new(ChatRole.User, longPrompt) };

        // Act
        var response = await client.GetResponseAsync(messages);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().Contain("Simulated AzureOpenAi");
        response.AdditionalProperties!["X-SmartRouter-Target"].Should().Be(ProviderKind.PremiumProvider.ToString());
        response.AdditionalProperties!["X-SmartRouter-FallbackApplied"].Should().Be(false);
    }

    [Fact]
    public async Task GetStreamingResponseAsync_Should_Stream_From_Fallback_When_Primary_Fails()
    {
        // Arrange
        var primaryFailing = new SimulatedProviderChatClient(
            ProviderKind.EconomicProvider, 
            "deepseek-chat", 
            alwaysFail: true);
        var fallbackMock = new SimulatedProviderChatClient(ProviderKind.PremiumProvider, "gpt-4o-mini");

        var client = new SmartRoutingChatClient(
            primaryClient: primaryFailing,
            fallbackClient: fallbackMock,
            routerStrategy: _routerStrategy,
            resiliencePipeline: _resiliencePipeline,
            healthTracker: _healthTracker);

        var messages = new List<ChatMessage> { new(ChatRole.User, "Stream with fallback") };

        // Act
        var updates = new List<ChatResponseUpdate>();
        await foreach (var update in client.GetStreamingResponseAsync(messages))
        {
            updates.Add(update);
        }

        // Assert
        updates.Should().NotBeEmpty();
        var fullText = string.Concat(updates.Select(u => u.Text));
        fullText.Should().Contain("Simulated AzureOpenAi");
    }
}
