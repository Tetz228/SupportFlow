using System.Net;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Modules.Organizations.Infrastructure.Persistence;

namespace SupportFlow.IntegrationTests;

[Collection(PostgreSqlCollection.Name)]
public sealed class CreateOrganizationEndpointTests(PostgreSqlFixture postgreSqlFixture)
    : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return postgreSqlFixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Post_WithValidRequest_CreatesOrganization()
    {
        // Arrange
        var applicationFactory = postgreSqlFixture.ApplicationFactory;

        using var httpClient = applicationFactory.CreateClient();

        const string expectedName = "Acme Corporation";
        var request = new CreateOrganizationRequest($"  {expectedName}  ");

        // Act
        using var response = await httpClient.PostAsJsonAsync("/api/organizations/", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadFromJsonAsync<CreateOrganizationResponse>();

        Assert.NotNull(responseContent);
        Assert.NotEqual(Guid.Empty, responseContent.Id);
        Assert.Equal(expectedName, responseContent.Name);
        Assert.Equal($"/api/organizations/{responseContent.Id}", response.Headers.Location?.OriginalString);

        await using var scope = applicationFactory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OrganizationsDbContext>();

        var organization = await dbContext.Organizations.SingleAsync();

        Assert.Equal(responseContent.Id, organization.Id);
        Assert.Equal(expectedName, organization.Name);
    }

    [Theory]
    [InlineData("      ")]
    [InlineData("Acme\0Corporation")]
    public async Task Post_WithInvalidName_ReturnsValidationProblemAndDoesNotCreateOrganization(string name)
    {
        // Arrange
        var applicationFactory = postgreSqlFixture.ApplicationFactory;

        using var httpClient = applicationFactory.CreateClient();

        var request = new CreateOrganizationRequest(name);

        // Act
        using var response = await httpClient.PostAsJsonAsync("/api/organizations/", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(MediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Contains(nameof(CreateOrganizationRequest.Name), problemDetails.Errors.Keys);
        ProblemDetailsAssertions.HasTraceId(problemDetails);

        var organizationsCount = await CountOrganizationsAsync(applicationFactory);

        Assert.Equal(0, organizationsCount);
    }

    private static async Task<int> CountOrganizationsAsync(SupportFlowApiFactory applicationFactory)
    {
        await using var scope = applicationFactory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OrganizationsDbContext>();

        return await dbContext.Organizations.CountAsync();
    }

    private sealed record CreateOrganizationRequest(string Name);

    private sealed record CreateOrganizationResponse(Guid Id, string Name);
}
