using AiProviderBenchmarker.Infrastructure.Configuration;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Factories.Strategies;

/// <summary>
/// Contrato de estratégia para instanciação de clientes IChatClient especializados por tipo ou protocolo de IA.
/// </summary>
public interface IChatClientStrategy
{
    /// <summary>
    /// Avalia se a estratégia sabe lidar com a configuração do provedor informado.
    /// </summary>
    bool CanHandle(AiProviderConfig config);

    /// <summary>
    /// Cria o cliente IChatClient devidamente configurado.
    /// </summary>
    IChatClient CreateClient(AiProviderConfig config);
}
