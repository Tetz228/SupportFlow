using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Modules.Organizations.Infrastructure.Persistence;

namespace SupportFlow.IntegrationTests;

public sealed class SupportFlowApiFactory : WebApplicationFactory<Program>
{
    private const string TestConnectionString = "Host=127.0.0.1;Database=supportflow-tests;Username=test;Password=test";

    public string ConnectionString { get; init; } = TestConnectionString;

    public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OrganizationsDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SupportFlow"] = ConnectionString
            });
        });

        return base.CreateHost(builder);
    }
}
