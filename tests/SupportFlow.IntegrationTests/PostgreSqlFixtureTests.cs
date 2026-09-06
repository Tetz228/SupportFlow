using Npgsql;

namespace SupportFlow.IntegrationTests;

public sealed class PostgreSqlFixtureTests(PostgreSqlFixture fixture)
    : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task ConnectionString_ConnectsToTestDatabase()
    {
        // Arrange
        await using var database = NpgsqlDataSource.Create(fixture.ConnectionString);

        await using var command = database.CreateCommand("SELECT current_database()");

        // Act
        var databaseName = await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal("supportflow_tests", databaseName);
    }

    [Fact]
    public async Task MigrateDatabaseAsync_CreatesOrganizationsTable()
    {
        // Arrange
        await using var applicationFactory = new SupportFlowApiFactory
        {
            ConnectionString = fixture.ConnectionString
        };

        await using var database = NpgsqlDataSource.Create(applicationFactory.ConnectionString);

        await using var command = database.CreateCommand("SELECT to_regclass('organizations.organizations')::text");

        // Act
        await applicationFactory.MigrateDatabaseAsync();

        var tableName = await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal("organizations.organizations", tableName);
    }
}
