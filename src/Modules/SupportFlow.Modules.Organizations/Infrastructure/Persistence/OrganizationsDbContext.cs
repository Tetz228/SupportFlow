using Microsoft.EntityFrameworkCore;
using SupportFlow.Modules.Organizations.Domain;

namespace SupportFlow.Modules.Organizations.Infrastructure.Persistence;

internal sealed class OrganizationsDbContext(DbContextOptions<OrganizationsDbContext> options) : DbContext(options)
{
    internal const string SchemaName = "organizations";

    internal DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationsDbContext).Assembly);
    }
}
