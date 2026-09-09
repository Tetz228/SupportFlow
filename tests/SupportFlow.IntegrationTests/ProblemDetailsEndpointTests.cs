using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace SupportFlow.IntegrationTests;

public sealed class ProblemDetailsEndpointTests(SupportFlowApiFactory applicationFactory)
    : IClassFixture<SupportFlowApiFactory>
{
    [Fact]
    public async Task GetUnknownRoute_ReturnsProblemDetails()
    {
        // Arrange
        using var httpClient = applicationFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/unknown");

        request.Headers.Accept.ParseAdd(MediaTypeNames.Application.ProblemJson);

        // Act
        using var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(MediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.False(string.IsNullOrWhiteSpace(problemDetails.Title));
        Assert.False(string.IsNullOrWhiteSpace(problemDetails.Type));
    }
}
