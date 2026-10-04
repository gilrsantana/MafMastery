using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartRouter.Infrastructure.Configuration;
using SmartRouter.Infrastructure.Extensions;

namespace SmartRouter.Tests.Configuration;

public class SmartRouterOptionsValidationTests
{
    [Fact]
    public void HasValidProviders_ShouldReturnTrue_WhenExactlyTwoProvidersAndOneIsPremium()
    {
        // Arrange
        var options = new SmartRouterOptions
        {
            Providers =
            [
                new AiProviderConfig { AiProviderName = "OpenRouter", PremiumTier = false },
                new AiProviderConfig { AiProviderName = "AzureOpenAi", PremiumTier = true }
            ]
        };

        // Act & Assert
        options.HasValidProviders().Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void HasValidProviders_ShouldReturnFalse_WhenProviderCountIsNotTwo(int count)
    {
        // Arrange
        var options = new SmartRouterOptions
        {
            Providers = Enumerable.Range(0, count)
                .Select(i => new AiProviderConfig { AiProviderName = $"Provider_{i}", PremiumTier = i == 0 })
                .ToList()
        };

        // Act & Assert
        options.HasValidProviders().Should().BeFalse();
    }

    [Fact]
    public void HasValidProviders_ShouldReturnFalse_WhenBothArePremium()
    {
        // Arrange
        var options = new SmartRouterOptions
        {
            Providers =
            [
                new AiProviderConfig { AiProviderName = "OpenRouter", PremiumTier = true },
                new AiProviderConfig { AiProviderName = "AzureOpenAi", PremiumTier = true }
            ]
        };

        // Act & Assert
        options.HasValidProviders().Should().BeFalse();
    }

    [Fact]
    public void HasValidProviders_ShouldReturnFalse_WhenNeitherIsPremium()
    {
        // Arrange
        var options = new SmartRouterOptions
        {
            Providers =
            [
                new AiProviderConfig { AiProviderName = "OpenRouter", PremiumTier = false },
                new AiProviderConfig { AiProviderName = "AzureOpenAi", PremiumTier = false }
            ]
        };

        // Act & Assert
        options.HasValidProviders().Should().BeFalse();
    }

    [Fact]
    public void AddSmartRouterInfrastructure_ShouldResolveOptions_WhenConfigurationIsValid()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["SmartRouter:Providers:0:AiProviderName"] = "OpenRouter",
            ["SmartRouter:Providers:0:PremiumTier"] = "false",
            ["SmartRouter:Providers:1:AiProviderName"] = "AzureOpenAi",
            ["SmartRouter:Providers:1:PremiumTier"] = "true",
            ["SmartRouter:UseSimulatedClientsIfUnconfigured"] = "true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSmartRouterInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<SmartRouterOptions>>().Value;

        // Assert
        options.Should().NotBeNull();
        options.Providers.Should().HaveCount(2);
        options.Providers.Should().ContainSingle(p => p.PremiumTier);
        options.Providers.Should().ContainSingle(p => !p.PremiumTier);
    }

    [Fact]
    public void AddSmartRouterInfrastructure_ShouldThrowOptionsValidationException_WhenProvidersAreInvalid()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["SmartRouter:Providers:0:AiProviderName"] = "OpenRouter",
            ["SmartRouter:Providers:0:PremiumTier"] = "false",
            ["SmartRouter:UseSimulatedClientsIfUnconfigured"] = "true"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSmartRouterInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var act = () => serviceProvider.GetRequiredService<IOptions<SmartRouterOptions>>().Value;

        // Assert
        act.Should().Throw<OptionsValidationException>()
            .WithMessage("*SmartRouterOptions deve conter exatamente dois provedores configurados, sendo um PremiumTier (true) e o outro não (false).*");
    }
}
