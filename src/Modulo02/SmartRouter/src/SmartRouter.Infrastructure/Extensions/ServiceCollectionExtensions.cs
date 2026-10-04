using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using Polly;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Models;
using SmartRouter.Domain.Services;
using SmartRouter.Domain.Strategies;
using SmartRouter.Infrastructure.Clients;
using SmartRouter.Infrastructure.Configuration;
using SmartRouter.Infrastructure.Health;
using SmartRouter.Infrastructure.Mock;
using SmartRouter.Infrastructure.Resilience;
using SmartRouter.Infrastructure.Strategies;

namespace SmartRouter.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    private const string EconomicRouterServiceAiKey = RouterServiceAiKey.Economic;
    private const string PremiumRouterServiceAiKey = RouterServiceAiKey.Premium;

    public static IServiceCollection AddSmartRouterInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureSmartRouterOptions(services, configuration);
        ConfigureHealthTracker(services);
        ConfigureRoutingStrategy(services);
        ConfigureResiliencePipeline(services);
        ConfigureEconomicAiClient(services);
        ConfigurePremiumAiClient(services);
        ConfigureSmartRoutingChatClient(services);

        return services;
    }

    private static void ConfigureSmartRouterOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmartRouterOptions>()
            .Bind(configuration.GetSection(SmartRouterOptions.SectionName))
            .Validate(
                options => options.HasValidProviders(),
                "SmartRouterOptions deve conter exatamente dois provedores configurados, sendo um PremiumTier (true) e o outro não (false).")
            .ValidateOnStart();
    }

    private static void ConfigureHealthTracker(IServiceCollection services)
    {
        services.AddSingleton<IProviderHealthTracker, ProviderHealthTracker>();
    }

    private static void ConfigureRoutingStrategy(IServiceCollection services)
    {
        services.AddSingleton<IModelRouterStrategy, HeuristicModelRouterStrategy>();
    }

    private static void ConfigureResiliencePipeline(IServiceCollection services)
    {
        services.AddSingleton<ResiliencePipeline>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SmartRouterOptions>>().Value;
            var healthTracker = sp.GetRequiredService<IProviderHealthTracker>();
            return SmartRouterResilienceFactory.CreatePipeline(options, healthTracker);
        });
    }

    private static void ConfigureEconomicAiClient(IServiceCollection services)
    {
        services.AddKeyedSingleton<IChatClient>(EconomicRouterServiceAiKey, (sp, _) =>
        {
            var options = sp.GetRequiredService<IOptions<SmartRouterOptions>>().Value;
            var economicAiProvider = options.GetEconomicAiProvider();

            if (!economicAiProvider.IsConfigured())
            {
                if (options.UseSimulatedClientsIfUnconfigured)
                {
                    return new SimulatedProviderChatClient(ProviderKind.EconomicProvider, economicAiProvider.DefaultModel);
                }

                throw new InvalidOperationException(
                    "Economic is not configured. Please check 'SmartRouter:Providers:Endpoint' and 'SmartRouter:Providers:ApiKey' in configuration or set 'UseSimulatedClientsIfUnconfigured' to true.");
            }

            var economicClientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(economicAiProvider.Endpoint)
            };

            var economicAiClient = new OpenAIClient(
                new ApiKeyCredential(economicAiProvider.ApiKey),
                economicClientOptions);

            return economicAiClient.GetChatClient(economicAiProvider.DefaultModel).AsIChatClient();
        });
    }

    private static void ConfigurePremiumAiClient(IServiceCollection services)
    {
        services.AddKeyedSingleton<IChatClient>(PremiumRouterServiceAiKey, (sp, _) =>
        {
            var options = sp.GetRequiredService<IOptions<SmartRouterOptions>>().Value;
            var premiumProvider = options.GetPremiumAiProvider();

            if (!premiumProvider.IsConfigured())
            {
                if (options.UseSimulatedClientsIfUnconfigured)
                {
                    return new SimulatedProviderChatClient(ProviderKind.PremiumProvider, premiumProvider.DefaultModel);
                }

                throw new InvalidOperationException(
                    "Premium is not configured. Please check 'SmartRouter:Providers:Endpoint' and 'SmartRouter:Providers:ApiKey' in configuration or set 'UseSimulatedClientsIfUnconfigured' to true.");
            }

            var premiumClientOptions = new OpenAIClientOptions
            {
                Endpoint = new Uri(premiumProvider.Endpoint)
            };

            var premiumAiClient = new OpenAIClient(
                new ApiKeyCredential(premiumProvider.ApiKey),
                premiumClientOptions);

            return premiumAiClient.GetChatClient(premiumProvider.DefaultModel).AsIChatClient();
        });
    }

    private static void ConfigureSmartRoutingChatClient(IServiceCollection services)
    {
        services.AddSingleton<IChatClient>(sp =>
        {
            var economyAiClient = sp.GetRequiredKeyedService<IChatClient>(EconomicRouterServiceAiKey);
            var premiumAiClient = sp.GetRequiredKeyedService<IChatClient>(PremiumRouterServiceAiKey);
            var routerStrategy = sp.GetRequiredService<IModelRouterStrategy>();
            var resiliencePipeline = sp.GetRequiredService<ResiliencePipeline>();
            var healthTracker = sp.GetRequiredService<IProviderHealthTracker>();

            return new SmartRoutingChatClient(
                primaryClient: economyAiClient,
                fallbackClient: premiumAiClient,
                routerStrategy: routerStrategy,
                resiliencePipeline: resiliencePipeline,
                healthTracker: healthTracker);
        });
    }
}
