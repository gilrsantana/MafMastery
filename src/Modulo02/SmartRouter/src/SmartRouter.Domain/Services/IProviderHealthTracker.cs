namespace SmartRouter.Domain.Services;

using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Models;

/// <summary>
/// Contrato para rastreamento de saúde, eventos de circuit breaker e telemetria dos provedores.
/// </summary>
public interface IProviderHealthTracker
{
    /// <summary>
    /// Notifica que uma chamada ao provedor foi concluída com sucesso.
    /// </summary>
    void RecordSuccess(ProviderKind provider);

    /// <summary>
    /// Notifica que uma chamada ao provedor falhou com uma exceção.
    /// </summary>
    void RecordFailure(ProviderKind provider, Exception exception);

    /// <summary>
    /// Notifica que o circuito foi aberto devido ao excesso de falhas.
    /// </summary>
    void RecordCircuitOpened(ProviderKind provider, TimeSpan breakDuration);

    /// <summary>
    /// Notifica que o circuito entrou em estado de teste (Half-Open).
    /// </summary>
    void RecordCircuitHalfOpened(ProviderKind provider);

    /// <summary>
    /// Notifica que o circuito foi reestabelecido e fechado com sucesso.
    /// </summary>
    void RecordCircuitClosed(ProviderKind provider);

    /// <summary>
    /// Obtém o instantâneo atual de saúde do provedor especificado.
    /// </summary>
    ProviderHealthSnapshot GetSnapshot(ProviderKind provider);
}
