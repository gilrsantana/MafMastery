namespace SmartRouter.Domain.Models;

using SmartRouter.Domain.Enums;

/// <summary>
/// Deterministic decision resolved by the model routing strategy.
/// </summary>
/// <param name="SelectedTier">Chosen route tier (Economic or Premium).</param>
/// <param name="TargetProvider">Target provider to which the invocation is initially dispatched.</param>
/// <param name="ModelIdentifier">Selected model identifier (e.g., deepseek/deepseek-chat or gpt-4o-mini).</param>
/// <param name="Reason">Technical justification of the selection for auditing and logs.</param>
public record RoutingDecision(
    RouteTier SelectedTier,
    ProviderKind TargetProvider,
    string ModelIdentifier,
    string Reason);
