using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoviePlatform.Modules.Catalog.Domain.Interfaces;
using MoviePlatform.Modules.Catalog.Infrastructure.Persistence;
using MoviePlatform.Modules.Catalog.Infrastructure.Repositories;

namespace MoviePlatform.Modules.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CatalogDbContext).Assembly.FullName));
        });

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();

        return services;
    }
}
