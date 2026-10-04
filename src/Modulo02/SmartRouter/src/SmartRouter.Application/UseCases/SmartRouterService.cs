using SmartRouter.Domain.Models;

namespace SmartRouter.Application.UseCases;

using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using SmartRouter.Application.Common;
using SmartRouter.Application.DTOs;
using SmartRouter.Domain.Enums;
using SmartRouter.Domain.Services;

/// <summary>
/// Implementação do serviço de aplicação que orquestra a execução de chat e métricas de gateway.
/// </summary>
public class SmartRouterService : ISmartRouterService
{
    private readonly IChatClient _chatClient;
    private readonly IProviderHealthTracker _healthTracker;
    private readonly IServiceProvider _serviceProvider;

    public SmartRouterService(
        IChatClient chatClient,
        IProviderHealthTracker healthTracker, IServiceProvider serviceProvider)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _healthTracker = healthTracker ?? throw new ArgumentNullException(nameof(healthTracker));
        _serviceProvider = serviceProvider;
    }

    public async Task<ChatResponseDto> CompleteChatAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var messages = MapMessages(request.Messages);
        var options = MapOptions(request);

        var stopwatch = Stopwatch.StartNew();
        var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken);
        stopwatch.Stop();


        var fallbackApplied = false;
        var targetProvider = "Unknown";
        var model = response.ModelId ?? request.Model ?? "default";

        if (response.AdditionalProperties != null)
        {
            if (response.AdditionalProperties.TryGetValue("X-SmartRouter-FallbackApplied", out var fallbackObj) &&
                fallbackObj is bool fb)
            {
                fallbackApplied = fb;
            }

            if (response.AdditionalProperties.TryGetValue("X-SmartRouter-Target", out var targetObj) &&
                targetObj is string targetStr)
            {
                targetProvider = targetStr;
            }
        }

        var responseText = response.Text ?? string.Empty;
        var inputTokens = (int?)response.Usage?.InputTokenCount;
        var outputTokens = (int?)response.Usage?.OutputTokenCount;

        return new ChatResponseDto(
            ResponseText: responseText,
            TargetProvider: targetProvider,
            Model: model,
            FallbackApplied: fallbackApplied,
            LatencyMs: stopwatch.ElapsedMilliseconds,
            InputTokens: inputTokens,
            OutputTokens: outputTokens);
    }

    public IAsyncEnumerable<ChatResponseUpdate> StreamChatAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var messages = MapMessages(request.Messages);
        var options = MapOptions(request);

        return _chatClient.GetStreamingResponseAsync(messages, options, cancellationToken);
    }

    public CircuitStatusDto GetCircuitStatus()
    {
        var snapshot = _healthTracker.GetSnapshot(ProviderKind.EconomicProvider);
        return new CircuitStatusDto(
            Provider: snapshot.Provider.ToString(),
            CircuitState: snapshot.CircuitState,
            IsOpen: snapshot.IsCircuitOpen,
            ConsecutiveFailures: snapshot.ConsecutiveFailures,
            TotalSuccesses: snapshot.TotalSuccesses,
            TotalFailures: snapshot.TotalFailures,
            LastFailureReason: snapshot.LastFailureReason,
            LastEvaluatedUtc: snapshot.LastCheckedUtc);
    }

    private static List<ChatMessage> MapMessages(IEnumerable<ChatMessageDto> dtos)
    {
        var list = new List<ChatMessage>();
        foreach (var dto in dtos)
        {
            var role = dto.Role.ToLowerInvariant() switch
            {
                "system" => ChatRole.System,
                "assistant" => ChatRole.Assistant,
                "tool" => ChatRole.Tool,
                _ => ChatRole.User
            };
            list.Add(new ChatMessage(role, dto.Content));
        }

        if (list.Count == 0)
        {
            list.Add(new ChatMessage(ChatRole.User, string.Empty));
        }

        return list;
    }

    private static ChatOptions MapOptions(ChatRequestDto request)
    {
        var options = new ChatOptions
        {
            ModelId = request.Model,
            Temperature = request.Temperature,
            MaxOutputTokens = request.MaxTokens,
            AdditionalProperties = new AdditionalPropertiesDictionary()
        };

        if (!string.IsNullOrWhiteSpace(request.RouteTierPreference))
        {
            options.AdditionalProperties["route-tier"] = request.RouteTierPreference;
        }

        return options;
    }

    public async Task<ProvidersHealthReportDto> CheckProvidersHealthAsync(bool liveProbe = false, CancellationToken cancellationToken = default)
    {
        var providers = new List<ProviderKind>
        {
            ProviderKind.EconomicProvider,
            ProviderKind.PremiumProvider
        };

        var items = new List<ProviderHealthItemDto>();

        foreach (var provider in providers)
        {
            var snapshot = _healthTracker.GetSnapshot(provider);
            long? latency = null;
            bool? isAlive = null;

            if (liveProbe)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(3));

                    var testClient = GetClientFor(provider);
                    await testClient.GetResponseAsync(
                        new[] { new ChatMessage(ChatRole.User, "ping") },
                        cancellationToken: cts.Token);
                    
                    sw.Stop();
                    latency = sw.ElapsedMilliseconds;
                    isAlive = true;
                }
                catch (Exception)
                {
                    sw.Stop();
                    latency = null;
                    isAlive = false;
                }
            }

            items.Add(new ProviderHealthItemDto(
                Provider: provider.ToString(),
                Tier: provider == ProviderKind.PremiumProvider ? "Premium" : "Economic",
                CircuitState: snapshot.CircuitState,
                IsCircuitOpen: snapshot.IsCircuitOpen,
                ConsecutiveFailures: snapshot.ConsecutiveFailures,
                TotalSuccesses: snapshot.TotalSuccesses,
                TotalFailures: snapshot.TotalFailures,
                LastFailureReason: snapshot.LastFailureReason,
                LastEvaluatedUtc: snapshot.LastCheckedUtc,
                ProbeLatencyMs: latency,
                IsAlive: isAlive
            ));
        }

        var overall = items.Any(p => p.IsCircuitOpen || p.IsAlive == false)
            ? ProviderHealthStatus.Degraded
            : ProviderHealthStatus.Healthy;

        return new ProvidersHealthReportDto(
            OverallStatus: overall,
            TimestampUtc: DateTimeOffset.UtcNow,
            Providers: items
        );
    }

    private IChatClient GetClientFor(ProviderKind provider)
    {
        var serviceKey = provider switch
        {
            ProviderKind.EconomicProvider => RouterServiceAiKey.Economic,
            ProviderKind.PremiumProvider => RouterServiceAiKey.Premium,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Provedor não suportado.")
        };

        return _serviceProvider.GetRequiredKeyedService<IChatClient>(serviceKey);

    }
}
