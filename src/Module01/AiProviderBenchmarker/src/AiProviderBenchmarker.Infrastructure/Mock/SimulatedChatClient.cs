using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Infrastructure.Mock;

/// <summary>
/// Custom implementation of IChatClient that simulates real-world LLM latency, TTFT, and streaming behavior.
/// Enables full offline testing of resilience, metrics, and CLI workflows without requiring paid API keys.
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
        // Simulate TTFT (Time To First Token)
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
        // No unmanaged resources
        GC.SuppressFinalize(this);
    }

    private static string GenerateSimulatedResponse(string prompt)
    {
        return $"[Synthesized Response via {_modelStaticId}]: Analyzing your request: \"{Truncate(prompt, 60)}\". " +
               "Based on Clean Architecture and SOLID principles, decoupling the inference pipeline " +
               "enables high maintainability, FinOps cost isolation, and resilience against external provider outages.";
    }

    private const string _modelStaticId = "SimulatedEngine";

    private static string Truncate(string val, int max) =>
        val.Length <= max ? val : val[..max] + "...";
}
