using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace SupportFlow.IntegrationTests;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("supportflow_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private SupportFlowApiFactory? _applicationFactory;
    private Respawner? _respawner;

    public string ConnectionString => _container.GetConnectionString();

    public SupportFlowApiFactory ApplicationFactory => _applicationFactory
        ?? throw new InvalidOperationException("PostgreSQL fixture has not been initialized.");

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _applicationFactory = new SupportFlowApiFactory
        {
            ConnectionString = ConnectionString
        };

        await _applicationFactory.MigrateDatabaseAsync();

        await using var connection = new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["organizations"],
            TablesToIgnore = [new Table("organizations", "__ef_migrations_history")]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException("PostgreSQL fixture has not been initialized.");
        }

        await using var connection = new NpgsqlConnection(ConnectionString);

        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        if (_applicationFactory is not null)
        {
            await _applicationFactory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}
