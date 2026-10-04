using Scalar.AspNetCore;
using SmartRouter.Application.Extensions;
using SmartRouter.Gateway.Endpoints;
using SmartRouter.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Clean Architecture module configuration and dependency injection
builder.Services.AddSmartRouterApplication();
builder.Services.AddSmartRouterInfrastructure(builder.Configuration);

// 2. Documentation and OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// 3. HTTP middleware pipeline and interactive documentation UI (Scalar)
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

// 4. Route and endpoint mapping
app.MapChatEndpoints();
app.MapDiagnosticEndpoints();

app.Run();

// Required to enable WebApplicationFactory in integration tests
public partial class Program { }
