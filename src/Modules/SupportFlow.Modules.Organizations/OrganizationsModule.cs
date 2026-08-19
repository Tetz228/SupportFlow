using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Modules.Organizations.Infrastructure.Persistence;

namespace SupportFlow.Modules.Organizations;

public static class OrganizationsModule
{
    public static IServiceCollection AddOrganizationsModule(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<OrganizationsDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
