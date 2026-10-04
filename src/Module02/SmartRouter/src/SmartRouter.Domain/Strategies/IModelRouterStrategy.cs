namespace SmartRouter.Domain.Strategies;

using Microsoft.Extensions.AI;
using SmartRouter.Domain.Models;

/// <summary>
/// Domain contract for intelligent routing and model selection strategies.
/// </summary>
public interface IModelRouterStrategy
{
    /// <summary>
    /// Evaluates conversational messages, context volume, and caller options to resolve the optimal route.
    /// </summary>
    /// <param name="messages">Chat message history supplied by the caller.</param>
    /// <param name="options">Chat options (hyperparameters, explicit tier routing properties).</param>
    /// <returns>Deterministic decision containing recommended route tier and target provider.</returns>
    RoutingDecision ResolveRoute(IEnumerable<ChatMessage> messages, ChatOptions? options = null);
}
