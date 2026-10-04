namespace SmartRouter.Infrastructure.Mock;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using SmartRouter.Domain.Enums;

/// <summary>
/// High-fidelity simulated client for local offline execution and deterministic testing.
/// </summary>
public class SimulatedProviderChatClient : IChatClient
{
    private readonly ProviderKind _provider;
    private readonly string _defaultModel;
    private readonly bool _alwaysFail;
    private readonly string? _failureMessage;

    public SimulatedProviderChatClient(
        ProviderKind provider, 
        string defaultModel,
        bool alwaysFail = false,
        string? failureMessage = null)
    {
        _provider = provider;
        _defaultModel = defaultModel;
        _alwaysFail = alwaysFail;
        _failureMessage = failureMessage;
    }

    private string ProviderDisplayName => _provider switch
    {
        ProviderKind.EconomicProvider => "OpenRouter",
        ProviderKind.PremiumProvider => "AzureOpenAi",
        _ => _provider.ToString()
    };

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        if (_alwaysFail)
        {
            throw new HttpRequestException(_failureMessage ?? $"Simulated network outage on {ProviderDisplayName}");
        }

        var promptSummary = chatMessages.LastOrDefault()?.Text ?? "(empty prompt)";
        var responseText = $"[Simulated {ProviderDisplayName} ({_defaultModel})] Response successfully generated for: '{promptSummary}'";

        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText))
        {
            ModelId = _defaultModel,
            Usage = new UsageDetails
            {
                InputTokenCount = promptSummary.Length / 4 + 5,
                OutputTokenCount = responseText.Length / 4 + 10
            },
            AdditionalProperties = new AdditionalPropertiesDictionary()
        };

        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_alwaysFail)
        {
            throw new HttpRequestException(_failureMessage ?? $"Simulated streaming outage on {ProviderDisplayName}");
        }

        var promptSummary = chatMessages.LastOrDefault()?.Text ?? "(empty prompt)";
        var chunks = new[]
        {
            $"[Simulated {ProviderDisplayName}] ",
            "Processing ",
            "your request ",
            "resiliently: ",
            $"'{promptSummary}'."
        };

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk)
            {
                ModelId = _defaultModel
            };
            await Task.Delay(20, cancellationToken);
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(SimulatedProviderChatClient))
        {
            return this;
        }
        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
