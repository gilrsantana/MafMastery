namespace SmartRouter.Application.Common;

using Microsoft.Extensions.AI;
using SmartRouter.Application.DTOs;

/// <summary>
/// Application service contract orchestrating conversational requests through SmartRouter.
/// </summary>
public interface ISmartRouterService
{
    /// <summary>
    /// Processes a standard chat request, returning the completed response DTO and gateway telemetry.
    /// </summary>
    Task<ChatResponseDto> CompleteChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a reactive chat request emitting streaming token updates.
    /// </summary>
    IAsyncEnumerable<ChatResponseUpdate> StreamChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the health and circuit breaker status for monitored providers.
    /// </summary>
    CircuitStatusDto GetCircuitStatus();

    /// <summary>
    /// Performs a health check on configured providers.
    /// </summary>
    /// <param name="liveProbe">Indicates whether an active latency probe should be executed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health report covering all configured providers.</returns>
    Task<ProvidersHealthReportDto> CheckProvidersHealthAsync(bool liveProbe = false, CancellationToken cancellationToken = default);
}
