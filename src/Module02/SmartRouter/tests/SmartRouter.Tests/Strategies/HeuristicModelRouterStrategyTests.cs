using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartRouter.Domain.Enums;
using SmartRouter.Infrastructure.Configuration;
using SmartRouter.Infrastructure.Strategies;

namespace SmartRouter.Tests.Strategies;

public class HeuristicModelRouterStrategyTests
{
    private readonly HeuristicModelRouterStrategy _strategy;

    public HeuristicModelRouterStrategyTests()
    {
        var options = Options.Create(new SmartRouterOptions
        {
            CharacterThresholdForPremiumTier = 1000,
            Providers =
            [
                new AiProviderConfig
                {
                    AiProviderName = "OpenRouter",
                    DefaultModel = "deepseek/deepseek-chat",
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
        });
        _strategy = new HeuristicModelRouterStrategy(options);
    }

    [Fact]
    public void ResolveRoute_Should_Select_Economy_When_Prompt_Is_Short()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "What is the capital of France?")
        };

        // Act
        var decision = _strategy.ResolveRoute(messages);

        // Assert
        decision.SelectedTier.Should().Be(RouteTier.Economic);
        decision.TargetProvider.Should().Be(ProviderKind.EconomicProvider);
        decision.ModelIdentifier.Should().Be("deepseek/deepseek-chat");
        decision.Reason.Should().Contain("Prompt size");
    }

    [Fact]
    public void ResolveRoute_Should_Select_Premium_When_Prompt_Exceeds_Character_Threshold()
    {
        // Arrange (more than 1000 characters)
        var longPrompt = new string('a', 1050);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, longPrompt)
        };

        // Act
        var decision = _strategy.ResolveRoute(messages);

        // Assert
        decision.SelectedTier.Should().Be(RouteTier.Premium);
        decision.TargetProvider.Should().Be(ProviderKind.PremiumProvider);
        decision.ModelIdentifier.Should().Be("gpt-4o-mini");
        decision.Reason.Should().Contain("threshold");
    }

    [Fact]
    public void ResolveRoute_Should_Select_Premium_When_Explicit_Preference_Provided()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Short message")
        };
        var options = new ChatOptions
        {
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                ["route-tier"] = "Premium"
            }
        };

        // Act
        var decision = _strategy.ResolveRoute(messages, options);

        // Assert
        decision.SelectedTier.Should().Be(RouteTier.Premium);
        decision.TargetProvider.Should().Be(ProviderKind.PremiumProvider);
        decision.Reason.Should().Contain("Explicit directive");
    }
}
