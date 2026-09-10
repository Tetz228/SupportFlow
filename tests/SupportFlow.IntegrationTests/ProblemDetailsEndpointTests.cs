using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SupportFlow.Modules.Organizations.Infrastructure.Persistence;

namespace SupportFlow.IntegrationTests;

public sealed class ProblemDetailsEndpointTests(SupportFlowApiFactory applicationFactory)
    : IClassFixture<SupportFlowApiFactory>
{
    private const string SensitiveExceptionMessage = "Sensitive internal exception details";

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
        ProblemDetailsAssertions.HasTraceId(problemDetails);
    }

    [Fact]
    public async Task Post_WhenUnhandledExceptionOccurs_ReturnsSafeProblemDetails()
    {
        // Arrange
        await using var failingApplicationFactory = applicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

            builder.ConfigureServices(services =>
            {
                services.AddDbContext<OrganizationsDbContext>((_, options) =>
                    options.AddInterceptors(new ThrowingSaveChangesInterceptor()));
            });
        });

        using var httpClient = failingApplicationFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/organizations/");
        request.Content = JsonContent.Create(new CreateOrganizationRequest("Acme Corporation"));
        request.Headers.Accept.ParseAdd(MediaTypeNames.Application.ProblemJson);

        // Act
        using var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(MediaTypeNames.Application.ProblemJson, response.Content.Headers.ContentType?.MediaType);

        var responseContent = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseContent,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(problemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.False(string.IsNullOrWhiteSpace(problemDetails.Title));
        Assert.False(string.IsNullOrWhiteSpace(problemDetails.Type));
        ProblemDetailsAssertions.HasTraceId(problemDetails);
        Assert.DoesNotContain(SensitiveExceptionMessage, responseContent, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), responseContent, StringComparison.Ordinal);
    }

    private sealed class ThrowingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(SensitiveExceptionMessage);
        }
    }

    private sealed record CreateOrganizationRequest(string Name);
}
