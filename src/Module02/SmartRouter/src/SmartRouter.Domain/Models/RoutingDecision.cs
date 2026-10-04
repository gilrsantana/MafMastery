namespace SmartRouter.Domain.Models;

using SmartRouter.Domain.Enums;

/// <summary>
/// Representa a decisão determinística tomada pela estratégia de roteamento.
/// </summary>
/// <param name="SelectedTier">Nível de rota escolhido (Economy ou Premium).</param>
/// <param name="TargetProvider">Provedor de destino para onde a chamada é direcionada inicialmente.</param>
/// <param name="ModelIdentifier">Identificador do modelo selecionado (ex.: deepseek/deepseek-chat ou gpt-4o-mini).</param>
/// <param name="Reason">Justificativa técnica da escolha para logs e auditoria.</param>
public record RoutingDecision(
    RouteTier SelectedTier,
    ProviderKind TargetProvider,
    string ModelIdentifier,
    string Reason);
