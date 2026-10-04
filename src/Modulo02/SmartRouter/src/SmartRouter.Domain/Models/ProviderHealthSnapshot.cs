namespace SmartRouter.Domain.Models;

using SmartRouter.Domain.Enums;

/// <summary>
/// Instantâneo do estado de saúde e telemetria do provedor monitorado pelo Circuit Breaker.
/// </summary>
/// <param name="Provider">Provedor avaliado.</param>
/// <param name="CircuitState">Nome do estado atual do circuito (Closed, Open, HalfOpen).</param>
/// <param name="IsCircuitOpen">Indica se o circuito está aberto (bloqueando chamadas diretas).</param>
/// <param name="ConsecutiveFailures">Quantidade de falhas acumuladas na janela amostral recente.</param>
/// <param name="TotalSuccesses">Total de requisições concluídas com sucesso.</param>
/// <param name="TotalFailures">Total de requisições que falharam.</param>
/// <param name="LastFailureReason">Mensagem de erro da última exceção interceptada.</param>
/// <param name="LastCheckedUtc">Horário UTC da última inspeção ou alteração de estado.</param>
public record ProviderHealthSnapshot(
    ProviderKind Provider,
    string CircuitState,
    bool IsCircuitOpen,
    int ConsecutiveFailures,
    long TotalSuccesses,
    long TotalFailures,
    string? LastFailureReason,
    DateTimeOffset LastCheckedUtc);
