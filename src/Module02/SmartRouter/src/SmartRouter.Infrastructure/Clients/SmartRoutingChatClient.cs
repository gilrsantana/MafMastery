namespace SmartRouter.Infrastructure.Clients;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Polly;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Services;
using SmartRouter.Domain.Strategies;

/// <summary>
/// Decorator de IChatClient que implementa roteamento dinâmico e resiliência com failover transparente.
/// </summary>
public class SmartRoutingChatClient : DelegatingChatClient
{
    private readonly IChatClient _fallbackClient;
    private readonly IModelRouterStrategy _routerStrategy;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly IProviderHealthTracker _healthTracker;

    public SmartRoutingChatClient(
        IChatClient primaryClient,
        IChatClient fallbackClient,
        IModelRouterStrategy routerStrategy,
        ResiliencePipeline resiliencePipeline,
        IProviderHealthTracker healthTracker) 
        : base(primaryClient)
    {
        _fallbackClient = fallbackClient ?? throw new ArgumentNullException(nameof(fallbackClient));
        _routerStrategy = routerStrategy ?? throw new ArgumentNullException(nameof(routerStrategy));
        _resiliencePipeline = resiliencePipeline ?? throw new ArgumentNullException(nameof(resiliencePipeline));
        _healthTracker = healthTracker ?? throw new ArgumentNullException(nameof(healthTracker));
    }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var messagesList = chatMessages.ToList();
        var decision = _routerStrategy.ResolveRoute(messagesList, options);

        // Se a estratégia já indicar a rota Premium, envia diretamente para o Azure OpenAI
        if (decision.SelectedTier == RouteTier.Premium)
        {
            var response = await _fallbackClient.GetResponseAsync(messagesList, options, cancellationToken);
            EnsureAdditionalProperties(response);
            response.AdditionalProperties!["X-SmartRouter-Target"] = ProviderKind.PremiumProvider.ToString();
            response.AdditionalProperties!["X-SmartRouter-FallbackApplied"] = false;
            return response;
        }

        // Rota Economy (OpenRouter): executa dentro do ResiliencePipeline do Polly v8
        try
        {
            return await _resiliencePipeline.ExecuteAsync(
                async ct =>
                {
                    try
                    {
                        var primaryResponse = await InnerClient.GetResponseAsync(messagesList, options, ct);
                        _healthTracker.RecordSuccess(ProviderKind.EconomicProvider);
                        EnsureAdditionalProperties(primaryResponse);
                        primaryResponse.AdditionalProperties!["X-SmartRouter-Target"] = ProviderKind.EconomicProvider.ToString();
                        primaryResponse.AdditionalProperties!["X-SmartRouter-FallbackApplied"] = false;
                        return primaryResponse;
                    }
                    catch (Exception ex)
                    {
                        _healthTracker.RecordFailure(ProviderKind.EconomicProvider, ex);
                        throw; // Re-lança para o Polly contabilizar na política de Circuit Breaker
                    }
                },
                cancellationToken);
        }
        catch (Exception)
        {
            // Ocorreu falha, timeout ou circuito aberto no primário: aciona fallback transparente
            var fallbackResponse = await _fallbackClient.GetResponseAsync(messagesList, options, cancellationToken);
            EnsureAdditionalProperties(fallbackResponse);
            fallbackResponse.AdditionalProperties!["X-SmartRouter-Target"] = ProviderKind.PremiumProvider.ToString();
            fallbackResponse.AdditionalProperties!["X-SmartRouter-FallbackApplied"] = true;
            return fallbackResponse;
        }
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messagesList = chatMessages.ToList();
        var decision = _routerStrategy.ResolveRoute(messagesList, options);

        // Se a rota for Premium ou o circuito do OpenRouter estiver aberto, transmite diretamente do Fallback
        if (decision.SelectedTier == RouteTier.Premium || _healthTracker.GetSnapshot(ProviderKind.EconomicProvider).IsCircuitOpen)
        {
            await foreach (var update in _fallbackClient.GetStreamingResponseAsync(messagesList, options, cancellationToken))
            {
                yield return update;
            }
            yield break;
        }

        // Tenta iniciar o stream do provedor primário
        IAsyncEnumerator<ChatResponseUpdate>? enumerator = null;
        var primaryFailed = false;

        try
        {
            enumerator = InnerClient.GetStreamingResponseAsync(messagesList, options, cancellationToken)
                                    .GetAsyncEnumerator(cancellationToken);
        }
        catch (Exception ex)
        {
            _healthTracker.RecordFailure(ProviderKind.EconomicProvider, ex);
            primaryFailed = true;
        }

        if (!primaryFailed && enumerator != null)
        {
            bool hasMore;
            try
            {
                hasMore = await enumerator.MoveNextAsync();
            }
            catch (Exception ex)
            {
                _healthTracker.RecordFailure(ProviderKind.EconomicProvider, ex);
                primaryFailed = true;
                hasMore = false;
            }

            if (!primaryFailed && hasMore)
            {
                _healthTracker.RecordSuccess(ProviderKind.EconomicProvider);
                do
                {
                    yield return enumerator.Current;
                    try
                    {
                        hasMore = await enumerator.MoveNextAsync();
                    }
                    catch (Exception)
                    {
                        // Falha abrupta no meio da transmissão
                        yield break;
                    }
                } while (hasMore);

                await enumerator.DisposeAsync();
                yield break;
            }

            if (enumerator != null)
            {
                await enumerator.DisposeAsync();
            }
        }

        // Falha antes de emitir tokens: aciona fallback transparente
        await foreach (var update in _fallbackClient.GetStreamingResponseAsync(messagesList, options, cancellationToken))
        {
            yield return update;
        }
    }

    private static void EnsureAdditionalProperties(ChatResponse response)
    {
        response.AdditionalProperties ??= new AdditionalPropertiesDictionary();
    }
}
