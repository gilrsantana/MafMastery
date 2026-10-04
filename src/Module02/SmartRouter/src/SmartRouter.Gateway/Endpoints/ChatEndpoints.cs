using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartRouter.Application.Common;
using SmartRouter.Application.DTOs;

namespace SmartRouter.Gateway.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/chat")
            .WithTags("Chat");

        // 1. Monolithic Chat Completions Endpoint
        group.MapPost("/completions", async (
            [FromBody] ChatRequestDto request,
            [FromServices] ISmartRouterService routerService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (request.Messages == null || !request.Messages.Any())
            {
                return Results.BadRequest(new { error = "Pelo menos uma mensagem é obrigatória." });
            }

            var response = await routerService.CompleteChatAsync(request, ct);

            // Adiciona Headers customizados exigidos na especificação da arquitetura
            httpContext.Response.Headers["X-SmartRouter-Target"] = response.TargetProvider;
            httpContext.Response.Headers["X-SmartRouter-FallbackApplied"] = response.FallbackApplied.ToString().ToLowerInvariant();
            httpContext.Response.Headers["X-SmartRouter-Latency-Ms"] = response.LatencyMs.ToString();

            return Results.Ok(response);
        })
        .WithName("CompleteChat")
        .WithSummary("Processa inferência com roteamento dinâmico e resiliência com failover transparente.")
        .Produces<ChatResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        // 2. Reactive Streaming Endpoint (Server-Sent Events)
        group.MapPost("/stream", async (
            [FromBody] ChatRequestDto request,
            [FromServices] ISmartRouterService routerService,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            if (request.Messages == null || !request.Messages.Any())
            {
                return Results.BadRequest(new { error = "Pelo menos uma mensagem é obrigatória." });
            }

            httpContext.Response.ContentType = "text/event-stream";
            httpContext.Response.Headers.CacheControl = "no-cache";
            httpContext.Response.Headers.Connection = "keep-alive";

            await foreach (var update in routerService.StreamChatAsync(request, ct))
            {
                var text = update.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    var payload = JsonSerializer.Serialize(new { delta = text });
                    await httpContext.Response.WriteAsync($"data: {payload}\n\n", ct);
                    await httpContext.Response.Body.FlushAsync(ct);
                }
            }

            await httpContext.Response.WriteAsync("data: [DONE]\n\n", ct);
            await httpContext.Response.Body.FlushAsync(ct);

            return Results.Empty;
        })
        .WithName("StreamChat")
        .WithSummary("Transmissão reativa de tokens via Server-Sent Events (SSE) com failover resiliente.")
        .Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
        .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}
