using System.Runtime.CompilerServices;
using AiProviderBenchmarker.Application.Common;
using AiProviderBenchmarker.Application.UseCases;
using AiProviderBenchmarker.Domain.Model;
using AiProviderBenchmarker.Domain.Services;
using FluentAssertions;
using Microsoft.Extensions.AI;
using NSubstitute;
using Xunit;

namespace AiProviderBenchmarker.Tests.Application;

public class RunBenchmarkHandlerTests
{
    private readonly IChatClientFactory _clientFactory = Substitute.For<IChatClientFactory>();
    private readonly ICostEstimator _costEstimator = Substitute.For<ICostEstimator>();
    private readonly RunBenchmarkHandler _sut;

    public RunBenchmarkHandlerTests()
    {
        _sut = new RunBenchmarkHandler(_clientFactory, _costEstimator);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExecuteAllProvidersConcurrently_AndReturnMetrics()
    {
        // Arrange
        var command = new RunBenchmarkCommand(
            Prompt: "Test Prompt",
            Options: new ChatOptions { MaxOutputTokens = 100 },
            ProvidersToBenchmark: ["OpenAi", "Simulated"]);

        _clientFactory.GetModelName("OpenAi").Returns("gpt-4o-mini");
        _clientFactory.GetModelName("Simulated").Returns("simulated-fast-llm");

        _clientFactory.CreateClient("OpenAi").Returns(new StubChatClient(["Hello", " ", "World"]));
        _clientFactory.CreateClient("Simulated").Returns(new StubChatClient(["Simulated", " ", "Output"]));

        _costEstimator.CalculateCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(0.00005m);

        // Act
        var results = await _sut.ExecuteAsync(command);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(m =>
        {
            m.Success.Should().BeTrue();
            m.TotalDuration.Should().BeGreaterThan(TimeSpan.Zero);
            m.GeneratedTextSnippet.Should().NotBeNullOrWhiteSpace();
            m.EstimatedCostUsd.Should().Be(0.00005m);
        });
    }

    [Fact]
    public async Task ExecuteAsync_WhenOneProviderFails_ShouldIsolateFailureAndKeepOthersSuccessful()
    {
        // Arrange: OpenAi throws HttpRequestException, but Simulated succeeds
        var command = new RunBenchmarkCommand(
            Prompt: "Fault tolerance prompt",
            Options: new ChatOptions { MaxOutputTokens = 50 },
            ProvidersToBenchmark: ["OpenAi", "Simulated"]);

        _clientFactory.GetModelName("OpenAi").Returns("gpt-4o-mini");
        _clientFactory.GetModelName("Simulated").Returns("simulated-fast-llm");

        _clientFactory.CreateClient("OpenAi")
            .Returns(new StubChatClient(new HttpRequestException("API connection timeout 504")));
        _clientFactory.CreateClient("Simulated")
            .Returns(new StubChatClient(["Valid", " ", "Response"]));

        // Act
        var results = await _sut.ExecuteAsync(command);

        // Assert
        results.Should().HaveCount(2);

        var failed = results.Single(r => r.Provider == "OpenAi");
        failed.Success.Should().BeFalse();
        failed.ErrorMessage.Should().Contain("API connection timeout 504");

        var succeeded = results.Single(r => r.Provider == "Simulated");
        succeeded.Success.Should().BeTrue();
        succeeded.GeneratedTextSnippet.Should().Be("Valid Response");
    }

    [Fact]
    public async Task ExecuteAsync_WithStreamProgress_ShouldReportChunks()
    {
        // Arrange
        var command = new RunBenchmarkCommand(
            Prompt: "Progress test",
            Options: new ChatOptions { MaxOutputTokens = 50 },
            ProvidersToBenchmark: ["Simulated"]);

        _clientFactory.GetModelName("Simulated").Returns("simulated-fast-llm");
        _clientFactory.CreateClient("Simulated").Returns(new StubChatClient(["Chunk1", "Chunk2"]));

        var receivedChunks = new List<(string Provider, string Chunk)>();
        var progress = new Progress<(string Provider, string Chunk)>(item =>
        {
            lock (receivedChunks)
            {
                receivedChunks.Add(item);
            }
        });

        // Act
        var results = await _sut.ExecuteAsync(command, streamProgress: progress);

        // Assert
        results.Should().ContainSingle();
        // Give event loop brief moment to flush progress dispatch
        await Task.Delay(50);
        receivedChunks.Should().HaveCount(2);
        receivedChunks[0].Chunk.Should().Be("Chunk1");
        receivedChunks[1].Chunk.Should().Be("Chunk2");
    }

    private sealed class StubChatClient : IChatClient
    {
        private readonly IEnumerable<string>? _chunks;
        private readonly Exception? _exceptionToThrow;

        public StubChatClient(IEnumerable<string> chunks) => _chunks = chunks;
        public StubChatClient(Exception exceptionToThrow) => _exceptionToThrow = exceptionToThrow;

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_exceptionToThrow is not null)
            {
                throw _exceptionToThrow;
            }

            if (_chunks is not null)
            {
                foreach (var chunk in _chunks)
                {
                    await Task.Yield();
                    yield return new ChatResponseUpdate(ChatRole.Assistant, chunk);
                }
            }
        }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }
}
