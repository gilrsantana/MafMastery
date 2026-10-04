namespace SmartRouter.Domain.Strategies;

using Microsoft.Extensions.AI;
using SmartRouter.Domain.Models;

/// <summary>
/// Contrato de domínio para a estratégia de seleção inteligente de rotas de modelos de IA.
/// </summary>
public interface IModelRouterStrategy
{
    /// <summary>
    /// Avalia a coleção de mensagens, o tamanho do contexto e as opções solicitadas para determinar a rota ideal.
    /// </summary>
    /// <param name="messages">Histórico de mensagens conversacionais enviadas pelo cliente.</param>
    /// <param name="options">Opções de chat (hiperparâmetros, propriedades adicionais).</param>
    /// <returns>Decisão determinística contendo a rota e provedor recomendados.</returns>
    RoutingDecision ResolveRoute(IEnumerable<ChatMessage> messages, ChatOptions? options = null);
}
