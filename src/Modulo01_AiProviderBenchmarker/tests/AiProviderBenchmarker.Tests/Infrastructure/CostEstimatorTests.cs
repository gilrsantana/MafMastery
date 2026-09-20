using AiProviderBenchmarker.Domain.Model;
using AiProviderBenchmarker.Infrastructure.Configuration;
using AiProviderBenchmarker.Infrastructure.Pricing;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AiProviderBenchmarker.Tests.Infrastructure;

public class CostEstimatorTests
{
    private readonly CostEstimator _sut;

    public CostEstimatorTests()
    {
        var options = new AiProvidersOptions
        {
            Providers =
            [
                new AiProviderConfig
                {
                    AiProviderName = "OpenAi",
                    DeploymentName = "gpt-4o-mini",
                    InputPricePerMillion = 0.15m,
                    OutputPricePerMillion = 0.60m
                },
                new AiProviderConfig
                {
                    AiProviderName = "Gemini",
                    DeploymentName = "gemini-3.5-flash-lite",
                    InputPricePerMillion = 0.30m,
                    OutputPricePerMillion = 2.50m
                },
                new AiProviderConfig
                {
                    AiProviderName = "Gemini",
                    DeploymentName = "gemini-3.5-flash",
                    InputPricePerMillion = 1.50m,
                    OutputPricePerMillion = 9.00m
                },
                new AiProviderConfig
                {
                    AiProviderName = "Ollama",
                    DeploymentName = "phi4",
                    InputPricePerMillion = 0.0m,
                    OutputPricePerMillion = 0.0m
                }
            ]
        };

        _sut = new CostEstimator(Options.Create(options));
    }

    [Theory]
    [InlineData("gpt-4o-mini", 1_000_000, 0, 0.15)]
    [InlineData("gpt-4o-mini", 0, 1_000_000, 0.60)]
    [InlineData("gpt-4o-mini", 10_000, 5_000, (10000 * 0.15 / 1_000_000) + (5000 * 0.60 / 1_000_000))]
    public void CalculateCost_ForOpenAiModels_ShouldComputeExactPricedValues(
        string modelName, int inputTokens, int outputTokens, double expectedCost)
    {
        // Act
        var cost = _sut.CalculateCost("OpenAi", modelName, inputTokens, outputTokens);

        // Assert
        ((double)cost).Should().BeApproximately(expectedCost, 0.000001);
    }

    [Fact]
    public void CalculateCost_ForOllama_ShouldAlwaysBeZero()
    {
        // Act
        var cost = _sut.CalculateCost("Ollama", "phi4", 100_000, 100_000);

        // Assert
        cost.Should().Be(0m);
    }

    [Fact]
    public void CalculateCost_WithZeroTokens_ShouldReturnZero()
    {
        // Act
        var cost = _sut.CalculateCost("OpenAi", "gpt-4o", 0, 0);

        // Assert
        cost.Should().Be(0m);
    }

    [Fact]
    public void CalculateCost_ForUnknownModel_ShouldFallbackToConservativeDefault()
    {
        // Act
        var cost = _sut.CalculateCost("AzureOpenAi", "custom-enterprise-model", 1_000_000, 1_000_000);

        // Assert: fallback is $0.50/M input + $1.50/M output = $2.00
        cost.Should().Be(2.00m);
    }

    [Fact]
    public void CalculateCost_ForMultipleModelsUnderSameProvider_ShouldResolveExactPrices()
    {
        // Act
        var liteCost = _sut.CalculateCost("Gemini", "gemini-3.5-flash-lite", 1_000_000, 1_000_000);
        var flashCost = _sut.CalculateCost("Gemini", "gemini-3.5-flash", 1_000_000, 1_000_000);

        // Assert
        liteCost.Should().Be(2.80m); // 0.30 + 2.50
        flashCost.Should().Be(10.50m); // 1.50 + 9.00
    }
}
