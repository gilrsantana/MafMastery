using AiProviderBenchmarker.Infrastructure.Configuration;
using FluentAssertions;
using Xunit;

namespace AiProviderBenchmarker.Tests.Infrastructure;

public class AiProvidersOptionsTests
{
    private readonly AiProvidersOptions _sut = new()
    {
        Providers =
        [
            new AiProviderConfig
            {
                AiProviderName = "OpenAi",
                DeploymentName = "gpt-4o-mini",
                ApiKey = "sk-test-openai",
                Enabled = true
            },
            new AiProviderConfig
            {
                AiProviderName = "AzureOpenAi",
                DeploymentName = "gpt-4o-mini",
                ApiKey = "sk-test-azure",
                Enabled = true
            },
            new AiProviderConfig
            {
                AiProviderName = "Gemini",
                DeploymentName = "gemini-3.5-flash-lite",
                ApiKey = "key-1",
                Enabled = true
            },
            new AiProviderConfig
            {
                AiProviderName = "Gemini",
                DeploymentName = "gemini-3.5-flash",
                ApiKey = "key-2",
                Enabled = true
            },
            new AiProviderConfig
            {
                AiProviderName = "Gemini",
                DeploymentName = "gemini-3.1-pro",
                ApiKey = "key-3",
                Enabled = true
            }
        ]
    };

    [Fact]
    public void GetProvider_ByExactDeploymentName_ShouldReturnSpecificConfig()
    {
        // Act
        var result = _sut.GetProvider("gemini-3.5-flash");

        // Assert
        result.Should().NotBeNull();
        result!.DeploymentName.Should().Be("gemini-3.5-flash");
        result.ApiKey.Should().Be("key-2");
    }

    [Fact]
    public void GetProvider_ByAiProviderName_ShouldReturnFirstMatchingConfig()
    {
        // Act
        var result = _sut.GetProvider("Gemini");

        // Assert
        result.Should().NotBeNull();
        result!.DeploymentName.Should().Be("gemini-3.5-flash-lite");
    }

    [Fact]
    public void GetProvider_WithUnknownKey_ShouldReturnNull()
    {
        // Act
        var result = _sut.GetProvider("NonExistentProvider");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveProviders_WhenGivenProviderName_ShouldReturnAllConfiguredModelsForThatProvider()
    {
        // Act
        var results = _sut.ResolveProviders(["Gemini"]);

        // Assert
        results.Should().HaveCount(3);
        results.Select(r => r.DeploymentName).Should().ContainInOrder(
            "gemini-3.5-flash-lite", "gemini-3.5-flash", "gemini-3.1-pro");
    }

    [Fact]
    public void ResolveProviders_WhenGivenSpecificDeploymentName_ShouldReturnOnlyThatDeployment()
    {
        // Act
        var results = _sut.ResolveProviders(["gemini-3.5-flash"]);

        // Assert
        results.Should().ContainSingle();
        results[0].DeploymentName.Should().Be("gemini-3.5-flash");
    }

    [Fact]
    public void ResolveProviders_WhenGivenMultipleKeys_ShouldReturnAllResolvedWithoutDuplicates()
    {
        // Act
        var results = _sut.ResolveProviders(["gpt-4o-mini", "gemini-3.5-flash-lite"]);

        // Assert
        results.Should().HaveCount(2);
        results.Select(r => r.DeploymentName).Should().Contain(["gpt-4o-mini", "gemini-3.5-flash-lite"]);
    }

    [Fact]
    public void GetProvider_ByCompositeKey_ShouldDistinguishSameDeploymentNameAcrossProviders()
    {
        // Act
        var openAiConfig = _sut.GetProvider("OpenAi:gpt-4o-mini");
        var azureConfig = _sut.GetProvider("AzureOpenAi:gpt-4o-mini");

        // Assert
        openAiConfig.Should().NotBeNull();
        openAiConfig!.AiProviderName.Should().Be("OpenAi");
        openAiConfig.ApiKey.Should().Be("sk-test-openai");

        azureConfig.Should().NotBeNull();
        azureConfig!.AiProviderName.Should().Be("AzureOpenAi");
        azureConfig.ApiKey.Should().Be("sk-test-azure");
    }
}
