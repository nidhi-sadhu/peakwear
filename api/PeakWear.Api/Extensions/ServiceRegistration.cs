using PeakWear.Core.Services;
using PeakWear.Data.Repositories;

namespace PeakWear.Api.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<UserRepository>()
                .AddClasses(c => c.Where(t => t.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.Scan(scan => scan
            .FromAssemblyOf<UserService>()
                .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
                .AsSelf()
                .WithScopedLifetime());

        return services;
    }
}