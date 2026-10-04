using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartRouter.Application.DTOs;

namespace SmartRouter.Tests.Endpoints;

public class GatewayIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GatewayIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_Should_Return_Success_Payload()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("SmartRouter Enterprise Gateway");
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task Health_Circuit_Endpoint_Should_Return_Circuit_Telemetry()
    {
        // Act
        var response = await _client.GetAsync("/health/circuit");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var status = await response.Content.ReadFromJsonAsync<CircuitStatusDto>();
        status.Should().NotBeNull();
        status!.Provider.Should().Be(SmartRouter.Domain.Enums.ProviderKind.EconomicProvider.ToString());
        status.CircuitState.Should().Be("Closed");
        status.IsOpen.Should().BeFalse();
    }

    [Fact]
    public async Task Chat_Completions_Should_Return_Target_And_Metadata_Headers()
    {
        // Arrange
        var request = new ChatRequestDto
        {
            Messages = new List<ChatMessageDto>
            {
                new("user", "Olá, preciso de um resumo rápido.")
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/chat/completions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("X-SmartRouter-Target").Should().BeTrue();
        response.Headers.Contains("X-SmartRouter-FallbackApplied").Should().BeTrue();
        response.Headers.Contains("X-SmartRouter-Latency-Ms").Should().BeTrue();

        var body = await response.Content.ReadFromJsonAsync<ChatResponseDto>();
        body.Should().NotBeNull();
        body!.ResponseText.Should().NotBeNullOrWhiteSpace();
        body.TargetProvider.Should().Be(SmartRouter.Domain.Enums.ProviderKind.EconomicProvider.ToString());
        body.FallbackApplied.Should().BeFalse();
    }

    [Fact]
    public async Task Chat_Completions_Should_Return_BadRequest_When_Messages_Are_Empty()
    {
        // Arrange
        var request = new ChatRequestDto
        {
            Messages = new List<ChatMessageDto>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/chat/completions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Chat_Stream_Should_Yield_Sse_Events_And_End_With_Done()
    {
        // Arrange
        var request = new ChatRequestDto
        {
            Messages = new List<ChatMessageDto>
            {
                new("user", "Teste de streaming reativo")
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/v1/chat/stream", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/event-stream");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("data:");
        content.Should().Contain("[DONE]");
    }
}
