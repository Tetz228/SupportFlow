using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportFlow.Modules.Organizations.Features.CreateOrganization;
using SupportFlow.Modules.Organizations.Infrastructure.Persistence;

namespace SupportFlow.Modules.Organizations;

public static class OrganizationsModule
{
    public static IServiceCollection AddOrganizationsModule(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddScoped<IValidator<CreateOrganizationRequest>, CreateOrganizationRequestValidator>();

        services.AddDbContext<OrganizationsDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    "__ef_migrations_history",
                    OrganizationsDbContext.SchemaName)));

        return services;
    }

    public static IEndpointRouteBuilder MapOrganizationsModule(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        var group = endpointRouteBuilder
            .MapGroup("/api/organizations")
            .WithTags("Organizations");

        group.MapCreateOrganizationEndpoint();

        return endpointRouteBuilder;
    }
}
