using Microsoft.Extensions.DependencyInjection;
using MoviePlatform.Shared.Kernel.Interfaces;
using MoviePlatform.Shared.Infrastructure.Persistence;

namespace MoviePlatform.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
