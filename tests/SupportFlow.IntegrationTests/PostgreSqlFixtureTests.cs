using Npgsql;

namespace SupportFlow.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class PostgreSqlFixtureTests(PostgreSqlFixture fixture)
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
    public async Task InitializeAsync_AppliesOrganizationsMigration()
    {
        // Arrange
        await using var database = NpgsqlDataSource.Create(fixture.ConnectionString);

        await using var command = database.CreateCommand("SELECT to_regclass('organizations.organizations')::text");

        // Act
        var tableName = await command.ExecuteScalarAsync();

        // Assert
        Assert.Equal("organizations.organizations", tableName);
    }

    [Fact]
    public async Task ResetDatabaseAsync_RemovesApplicationDataAndPreservesMigrationHistory()
    {
        // Arrange
        await using var database = NpgsqlDataSource.Create(fixture.ConnectionString);

        await using (var insertCommand = database.CreateCommand(
                         "INSERT INTO organizations.organizations (id, name) VALUES (@id, @name)"))
        {
            insertCommand.Parameters.AddWithValue("id", Guid.CreateVersion7());
            insertCommand.Parameters.AddWithValue("name", "Acme Corporation");

            await insertCommand.ExecuteNonQueryAsync();
        }

        // Act
        await fixture.ResetDatabaseAsync();

        // Assert
        await using var organizationsCountCommand = database.CreateCommand(
            "SELECT COUNT(*) FROM organizations.organizations");
        await using var migrationsCountCommand = database.CreateCommand(
            "SELECT COUNT(*) FROM organizations.__ef_migrations_history");

        var organizationsCount = (long)(await organizationsCountCommand.ExecuteScalarAsync())!;
        var migrationsCount = (long)(await migrationsCountCommand.ExecuteScalarAsync())!;

        Assert.Equal(0, organizationsCount);
        Assert.True(migrationsCount > 0);
    }
}
