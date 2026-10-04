namespace SmartRouter.Application.Common;

using Microsoft.Extensions.AI;
using SmartRouter.Application.DTOs;

/// <summary>
/// Contrato do serviço de aplicação que orquestra as chamadas conversacionais através do SmartRouter.
/// </summary>
public interface ISmartRouterService
{
    /// <summary>
    /// Processa uma requisição de chat tradicional (monolítica), retornando o DTO completo de resposta e métricas.
    /// </summary>
    Task<ChatResponseDto> CompleteChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa uma requisição de chat reativa emitindo chunks de tokens via streaming assíncrono.
    /// </summary>
    IAsyncEnumerable<ChatResponseUpdate> StreamChatAsync(ChatRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o status de saúde e do circuit breaker do provedor monitorado.
    /// </summary>
    CircuitStatusDto GetCircuitStatus();

    /// <summary>
    /// Realiza uma verificação de saúde nos providers configurados.
    /// </summary>
    /// <param name="liveProbe">Indica se deve ser realizado um probe de latência.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Report de saúde dos providers.</returns>
    Task<ProvidersHealthReportDto> CheckProvidersHealthAsync(bool liveProbe = false, CancellationToken cancellationToken = default);
}
