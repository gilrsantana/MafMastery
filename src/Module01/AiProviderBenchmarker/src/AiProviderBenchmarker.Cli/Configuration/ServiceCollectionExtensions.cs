using AiProviderBenchmarker.Application.Common;
using AiProviderBenchmarker.Application.UseCases;
using AiProviderBenchmarker.Cli.Arguments;
using AiProviderBenchmarker.Cli.Runners;
using AiProviderBenchmarker.Domain.Services;
using AiProviderBenchmarker.Infrastructure.Configuration;
using AiProviderBenchmarker.Infrastructure.Factories;
using AiProviderBenchmarker.Infrastructure.Factories.Strategies;
using AiProviderBenchmarker.Infrastructure.Pricing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;

namespace AiProviderBenchmarker.Cli.Configuration;

/// <summary>
/// Centralized Dependency Injection (IoC) configuration for the CLI application.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IConfiguration BuildAppConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                ??  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                                ?? "Development";
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(environment == "Development" ? "appsettings.Development.json" : "appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
    }

    public static IServiceCollection AddCliServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.AddSingleton(configuration);
        services.Configure<AiProvidersOptions>(configuration.GetSection(AiProvidersOptions.SectionName));

        // Domain & Infrastructure
        services.AddSingleton<ICostEstimator, CostEstimator>();

        // Client Creation Strategies (Strategy Pattern)
        services.AddSingleton<IChatClientStrategy, SimulatedClientStrategy>();
        services.AddSingleton<IChatClientStrategy, AzureOpenAiClientStrategy>();
        services.AddSingleton<IChatClientStrategy, OpenAiCompatibleClientStrategy>();

        services.AddSingleton<IChatClientFactory, ChatClientFactory>();
        services.AddTransient<IRunBenchmarkUseCase, RunBenchmarkHandler>();

        // CLI Parsing and Runners
        services.AddSingleton<CliArgumentParser>();
        services.AddTransient<InteractiveBenchmarkRunner>();
        services.AddTransient<NonInteractiveBenchmarkRunner>();
        services.AddTransient<CliApp>();

        return services;
    }
}
