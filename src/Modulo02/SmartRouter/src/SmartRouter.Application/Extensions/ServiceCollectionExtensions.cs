using Microsoft.Extensions.DependencyInjection;
using SmartRouter.Application.Common;
using SmartRouter.Application.UseCases;

namespace SmartRouter.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartRouterApplication(this IServiceCollection services)
    {
        services.AddScoped<ISmartRouterService, SmartRouterService>();
        return services;
    }
}
