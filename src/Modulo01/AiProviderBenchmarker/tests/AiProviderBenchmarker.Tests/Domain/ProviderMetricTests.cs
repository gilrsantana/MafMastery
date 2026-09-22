using AiProviderBenchmarker.Domain.Model;
using FluentAssertions;
using Xunit;

namespace AiProviderBenchmarker.Tests.Domain;

public class ProviderMetricTests
{
    [Fact]
    public void TokensPerSecond_ShouldCalculateCorrectly_WhenGenerationTimeIsPositive()
    {
        // Arrange
        // Total duration: 3.0s, TTFT: 1.0s -> Active generation time: 2.0s
        // Output tokens: 100 -> TPS = 100 / 2.0 = 50.0
        var metric = new ProviderMetric(
            Provider: "OpenAi",
            ModelName: "gpt-4o-mini",
            Success: true,
            TimeToFirstToken: TimeSpan.FromSeconds(1),
            TotalDuration: TimeSpan.FromSeconds(3),
            InputTokens: 50,
            OutputTokens: 100,
            EstimatedCostUsd: 0.0001m,
            GeneratedTextSnippet: "Hello world");

        // Act
        var tps = metric.TokensPerSecond;

        // Assert
        tps.Should().BeApproximately(50.0, 0.001);
    }

    [Fact]
    public void TokensPerSecond_ShouldFallbackToTotalDuration_WhenTtftEqualsOrExceedsTotalDuration()
    {
        // Arrange
        var metric = new ProviderMetric(
            Provider: "Simulated",
            ModelName: "mock",
            Success: true,
            TimeToFirstToken: TimeSpan.FromSeconds(2),
            TotalDuration: TimeSpan.FromSeconds(2),
            InputTokens: 10,
            OutputTokens: 20,
            EstimatedCostUsd: 0m,
            GeneratedTextSnippet: "Test");

        // Act
        var tps = metric.TokensPerSecond;

        // Assert
        // When generationTime <= 0, it falls back to TotalDuration (2.0s): 20 / 2.0 = 10.0
        tps.Should().BeApproximately(10.0, 0.001);
    }

    [Fact]
    public void TokensPerSecond_ShouldReturnZero_WhenOutputTokensIsZero()
    {
        // Arrange
        var metric = new ProviderMetric(
            Provider: "Ollama",
            ModelName: "phi4",
            Success: true,
            TimeToFirstToken: TimeSpan.FromSeconds(0.5),
            TotalDuration: TimeSpan.FromSeconds(1.5),
            InputTokens: 30,
            OutputTokens: 0,
            EstimatedCostUsd: 0m,
            GeneratedTextSnippet: "");

        // Act
        var tps = metric.TokensPerSecond;

        // Assert
        tps.Should().Be(0);
    }

    [Fact]
    public void FailedMetric_ShouldCarryErrorMessage()
    {
        // Arrange
        var metric = new ProviderMetric(
            Provider: "AzureOpenAi",
            ModelName: "gpt-4o",
            Success: false,
            TimeToFirstToken: TimeSpan.Zero,
            TotalDuration: TimeSpan.FromSeconds(0.8),
            InputTokens: 0,
            OutputTokens: 0,
            EstimatedCostUsd: 0m,
            GeneratedTextSnippet: "",
            ErrorMessage: "Connection refused");

        // Assert
        metric.Success.Should().BeFalse();
        metric.ErrorMessage.Should().Be("Connection refused");
        metric.TokensPerSecond.Should().Be(0);
    }
}
