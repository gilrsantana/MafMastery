using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Mock;

/// <summary>
/// Implementação customizada de IChatClient que simula a latência, TTFT e streaming de um modelo de linguagem real.
/// Permite testar toda a infraestrutura, resiliência e CLI offline sem depender de chaves de API pagas.
/// </summary>
public class SimulatedChatClient : IChatClient
{
    private readonly string _modelId;
    private readonly int _minTtftMs;
    private readonly int _maxTtftMs;
    private readonly int _tokensPerSecond;
    private static readonly Random Rnd = new();

    public SimulatedChatClient(
        string modelId = "simulated-fast-llm",
        int minTtftMs = 180,
        int maxTtftMs = 380,
        int tokensPerSecond = 45)
    {
        _modelId = modelId;
        _minTtftMs = minTtftMs;
        _maxTtftMs = maxTtftMs;
        _tokensPerSecond = Math.Max(1, tokensPerSecond);
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var delay = Rnd.Next(_minTtftMs, _maxTtftMs) + (1000 / _tokensPerSecond) * 15;
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

        var prompt = chatMessages.LastOrDefault()?.Text ?? string.Empty;
        var text = GenerateSimulatedResponse(prompt);

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, text))
        {
            ModelId = _modelId
        };
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Simula TTFT (Time To First Token)
        var ttft = Rnd.Next(_minTtftMs, _maxTtftMs);
        await Task.Delay(ttft, cancellationToken).ConfigureAwait(false);

        var prompt = chatMessages.LastOrDefault()?.Text ?? string.Empty;
        var fullText = GenerateSimulatedResponse(prompt);
        var words = fullText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var delayPerWord = Math.Max(10, 1000 / _tokensPerSecond);

        foreach (var word in words)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return new ChatResponseUpdate(ChatRole.Assistant, word + " ")
            {
                ModelId = _modelId
            };

            await Task.Delay(delayPerWord, cancellationToken).ConfigureAwait(false);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(SimulatedChatClient))
            return this;

        return null;
    }

    public void Dispose()
    {
        // Sem recursos não gerenciados
        GC.SuppressFinalize(this);
    }

    private static string GenerateSimulatedResponse(string prompt)
    {
        return $"[Resposta Sintetizada via {_modelStaticId}]: Analisando sua solicitação: \"{Truncate(prompt, 60)}\". " +
               "Com base nos princípios de Clean Architecture e SOLID, a separação do pipeline de inferência " +
               "permite alta manutenibilidade, isolamento de custos de FinOps e resiliência a falhas de provedores externos.";
    }

    private const string _modelStaticId = "SimulatedEngine";

    private static string Truncate(string val, int max) =>
        val.Length <= max ? val : val[..max] + "...";
}
