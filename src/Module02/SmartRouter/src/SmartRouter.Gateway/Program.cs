using Scalar.AspNetCore;
using SmartRouter.Application.Extensions;
using SmartRouter.Gateway.Endpoints;
using SmartRouter.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração e injeção de dependência dos módulos Clean Architecture
builder.Services.AddSmartRouterApplication();
builder.Services.AddSmartRouterInfrastructure(builder.Configuration);

// 2. Documentação e OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// 3. Pipeline de middleware HTTP e UI Interativa de Documentação (Scalar)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("SmartRouter — Enterprise AI Gateway")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

// 4. Mapeamento de rotas e endpoints
app.MapChatEndpoints();
app.MapDiagnosticEndpoints();

app.Run();

// Necessário para habilitar WebApplicationFactory nos testes de integração
public partial class Program { }
