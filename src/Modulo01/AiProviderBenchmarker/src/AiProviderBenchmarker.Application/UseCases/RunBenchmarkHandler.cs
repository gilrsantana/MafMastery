using System.Diagnostics;
using System.Text;
using AiProviderBenchmarker.Application.Common;
using AiProviderBenchmarker.Domain.Model;
using AiProviderBenchmarker.Domain.Services;
using Microsoft.Extensions.AI;

namespace AiProviderBenchmarker.Application.UseCases;

/// <summary>
/// Implementação do caso de uso de benchmark de IA com execução paralela, medição de TTFT e resiliência.
/// </summary>
public class RunBenchmarkHandler : IRunBenchmarkUseCase
{
    private readonly IChatClientFactory _clientFactory;
    private readonly ICostEstimator _costEstimator;

    public RunBenchmarkHandler(IChatClientFactory clientFactory, ICostEstimator costEstimator)
    {
        _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        _costEstimator = costEstimator ?? throw new ArgumentNullException(nameof(costEstimator));
    }

    public async Task<IReadOnlyList<ProviderMetric>> ExecuteAsync(
        RunBenchmarkCommand command,
        IProgress<(string Provider, string Chunk)>? streamProgress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var providers = command.ProvidersToBenchmark.Count > 0
            ? command.ProvidersToBenchmark
            : _clientFactory.GetConfiguredProviders();

        if (providers.Count == 0)
        {
            return Array.Empty<ProviderMetric>();
        }

        var tasks = providers.Select(provider => 
            BenchmarkSingleProviderAsync(provider, command.Prompt, command.Options, streamProgress, cancellationToken));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToList();
    }

    private async Task<ProviderMetric> BenchmarkSingleProviderAsync(
        string providerKey,
        string prompt,
        ChatOptions options,
        IProgress<(string Provider, string Chunk)>? streamProgress,
        CancellationToken cancellationToken)
    {
        var providerName = _clientFactory.GetProviderName(providerKey);
        if (string.IsNullOrWhiteSpace(providerName))
        {
            providerName = providerKey;
        }
        var modelName = _clientFactory.GetModelName(providerKey);
        var stopwatch = Stopwatch.StartNew();
        TimeSpan? timeToFirstToken = null;
        var accumulatedText = new StringBuilder();
        var reportedOutputTokens = 0;
        var reportedInputTokens = 0;

        try
        {
            using var client = _clientFactory.CreateClient(providerKey);
            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, prompt)
            };

            // Clona as opções para evitar mutações concorrentes entre clientes
            var providerOptions = CloneOptions(options);

            await foreach (var update in client.GetStreamingResponseAsync(messages, providerOptions, cancellationToken).ConfigureAwait(false))
            {
                if (!timeToFirstToken.HasValue)
                {
                    timeToFirstToken = stopwatch.Elapsed;
                }

                if (!string.IsNullOrEmpty(update.Text))
                {
                    accumulatedText.Append(update.Text);
                    streamProgress?.Report((providerKey, update.Text));
                }
            }

            stopwatch.Stop();
            var totalDuration = stopwatch.Elapsed;
            var finalTtft = timeToFirstToken ?? totalDuration;

            // Estimativa de tokens caso o adaptador não tenha retornado metadados exatos no streaming
            var generatedText = accumulatedText.ToString();
            var inputTokens = reportedInputTokens > 0 ? reportedInputTokens : EstimateTokens(prompt);
            var outputTokens = reportedOutputTokens > 0 ? reportedOutputTokens : EstimateTokens(generatedText);

            var estimatedCost = _costEstimator.CalculateCost(providerName, modelName, inputTokens, outputTokens);

            var snippet = generatedText.Length > 120 
                ? string.Concat(generatedText.AsSpan(0, 117), "...") 
                : generatedText;

            return new ProviderMetric(
                Provider: providerName,
                ModelName: modelName,
                TimeToFirstToken: finalTtft,
                TotalDuration: totalDuration,
                InputTokens: inputTokens,
                OutputTokens: outputTokens,
                EstimatedCostUsd: estimatedCost,
                GeneratedTextSnippet: snippet.Trim(),
                Success: true);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            return new ProviderMetric(
                Provider: providerName,
                ModelName: modelName,
                TimeToFirstToken: timeToFirstToken ?? stopwatch.Elapsed,
                TotalDuration: stopwatch.Elapsed,
                InputTokens: EstimateTokens(prompt),
                OutputTokens: 0,
                EstimatedCostUsd: 0m,
                GeneratedTextSnippet: string.Empty,
                Success: false,
                ErrorMessage: "Operação cancelada pelo usuário.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new ProviderMetric(
                Provider: providerName,
                ModelName: modelName,
                TimeToFirstToken: timeToFirstToken ?? stopwatch.Elapsed,
                TotalDuration: stopwatch.Elapsed,
                InputTokens: EstimateTokens(prompt),
                OutputTokens: 0,
                EstimatedCostUsd: 0m,
                GeneratedTextSnippet: string.Empty,
                Success: false,
                ErrorMessage: ex.Message);
        }
    }

    private static ChatOptions CloneOptions(ChatOptions source)
    {
        return new ChatOptions
        {
            Temperature = source.Temperature,
            TopP = source.TopP,
            TopK = source.TopK,
            MaxOutputTokens = source.MaxOutputTokens,
            Seed = source.Seed,
            StopSequences = source.StopSequences != null ? new List<string>(source.StopSequences) : null
        };
    }

    /// <summary>
    /// Heurística padrão da indústria para estimativa de tokens em língua ocidental (aprox. 4 caracteres por token).
    /// </summary>
    private static int EstimateTokens(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return Math.Max(1, (int)Math.Ceiling(text.Length / 4.0));
    }
}
