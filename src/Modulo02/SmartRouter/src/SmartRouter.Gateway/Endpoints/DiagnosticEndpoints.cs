using Microsoft.AspNetCore.Mvc;
using SmartRouter.Application.Common;
using SmartRouter.Application.DTOs;
using SmartRouter.Domain.Models;

namespace SmartRouter.Gateway.Endpoints;

public static class DiagnosticEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/health")
            .WithTags("Diagnostics");

        group.MapGet("/", () => Results.Ok(new
        {
            status = "Healthy",
            gateway = "SmartRouter Enterprise Gateway",
            version = "2.0.0-LTS",
            timestampUtc = DateTime.UtcNow
        }))
        .WithName("GetGatewayHealth")
        .WithSummary("Verifica a prontidão e integridade do Gateway.")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/circuit", ([FromServices] ISmartRouterService routerService) =>
        {
            var status = routerService.GetCircuitStatus();
            return Results.Ok(status);
        })
        .WithName("GetCircuitStatus")
        .WithSummary("Consulta a telemetria em tempo real do Circuit Breaker do provedor primário.")
        .Produces<CircuitStatusDto>(StatusCodes.Status200OK);

        group.MapGet("/providers", async (
            [FromServices] ISmartRouterService routerService,
            [FromQuery] bool? live,
            CancellationToken cancellationToken) =>
            {
                var report = await routerService.CheckProvidersHealthAsync(live ?? false, cancellationToken);
                return report.OverallStatus == ProviderHealthStatus.Healthy
                    ? Results.Ok(report)
                    : Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        )
        .WithName("GetProvidersHealth")
        .WithSummary("Consulta a telemetria ou executa uma sonda ativa em todos os provedores.")
        .Produces<ProvidersHealthReportDto>(StatusCodes.Status200OK)
        .Produces<ProvidersHealthReportDto>(StatusCodes.Status503ServiceUnavailable);

        return app;
    }
}
