namespace SmartRouter.Infrastructure.Strategies;

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Models;
using SmartRouter.Domain.Strategies;
using SmartRouter.Infrastructure.Configuration;

/// <summary>
/// Heuristic routing strategy based on contextual character volume and explicit tier preferences.
/// </summary>
public class HeuristicModelRouterStrategy : IModelRouterStrategy
{
    private readonly SmartRouterOptions _options;

    public HeuristicModelRouterStrategy(IOptions<SmartRouterOptions> options)
    {
        _options = options?.Value ?? new SmartRouterOptions();
    }

    public RoutingDecision ResolveRoute(IEnumerable<ChatMessage> messages, ChatOptions? chatOptions = null)
    {
        var (selectedValidValue, tierStr) = HasSelectedProvider(chatOptions);

        if (selectedValidValue)
            return SelectedTier(tierStr, _options);

        int totalChars = CalculateTotalCharacters(messages);

        var reason = string.Empty;

        if (totalChars >= _options.CharacterThresholdForPremiumTier)
        {
            reason = $"Prompt size ({totalChars} chars) meets or exceeds premium threshold ({_options.CharacterThresholdForPremiumTier} chars)";
            return ReturnPremiumTier(_options, reason);
        }

        reason = $"Prompt size ({totalChars} chars) within economy tier threshold (< {_options.CharacterThresholdForPremiumTier} chars)";
        return ReturnEconomyTier(_options, reason);
    }

    private static (bool selectedValidValue, string tierStrOut) HasSelectedProvider(ChatOptions? chatOptions)
    {
        if (chatOptions is null)
            return (false, string.Empty);

        var selectedValidValue = chatOptions.AdditionalProperties != null &&
            chatOptions.AdditionalProperties.TryGetValue("route-tier", out var tierObj) &&
            tierObj is string tierStrOut &&
            (tierStrOut.Equals("premium", StringComparison.OrdinalIgnoreCase) || tierStrOut.Equals("economy", StringComparison.OrdinalIgnoreCase));

        tierStrOut = selectedValidValue
            ? chatOptions!.AdditionalProperties!["route-tier"] as string ?? string.Empty
            : string.Empty;

        return (selectedValidValue, tierStrOut);
    }

    private static RoutingDecision SelectedTier(string tierStr, SmartRouterOptions options)
    {
        return tierStr.Equals("premium", StringComparison.OrdinalIgnoreCase)
            ? ReturnPremiumTier(options, "Explicit directive: user requested 'premium' route tier")
            : ReturnEconomyTier(options, "Explicit directive: user requested 'economy' route tier");
    }

    private static RoutingDecision ReturnPremiumTier(SmartRouterOptions options, string reason)
    {
        return new RoutingDecision(
            SelectedTier: RouteTier.Premium,
            TargetProvider: ProviderKind.PremiumProvider,
            ModelIdentifier: options.GetPremiumAiProvider().DefaultModel,
            Reason: reason);
    }

    private static RoutingDecision ReturnEconomyTier(SmartRouterOptions options, string reason)
    {
        return new RoutingDecision(
            SelectedTier: RouteTier.Economic,
            TargetProvider: ProviderKind.EconomicProvider,
            ModelIdentifier: options.GetEconomicAiProvider().DefaultModel,
            Reason: reason);
    }

    private static int CalculateTotalCharacters(IEnumerable<ChatMessage> messages)
    {
        var totalChars = 0;
        foreach (var message in messages)
        {
            totalChars += message.Text?.Length ?? 0;
        }

        return totalChars;
    }

}