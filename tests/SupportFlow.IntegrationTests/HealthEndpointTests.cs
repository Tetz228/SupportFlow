using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SupportFlow.IntegrationTests;

public sealed class HealthEndpointTests(WebApplicationFactory<Program> webApplicationFactory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Arrange
        using var httpClient = webApplicationFactory.CreateClient();
        
        // Act
        using var response = await httpClient.GetAsync("/health");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}