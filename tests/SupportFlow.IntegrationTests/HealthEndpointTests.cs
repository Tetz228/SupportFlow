using System.Net;

namespace SupportFlow.IntegrationTests;

public sealed class HealthEndpointTests(SupportFlowApiFactory applicationFactory)
    : IClassFixture<SupportFlowApiFactory>
{
    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Arrange
        using var httpClient = applicationFactory.CreateClient();

        // Act
        using var response = await httpClient.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
