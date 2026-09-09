using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace SupportFlow.IntegrationTests;

public sealed class OpenApiEndpointTests(SupportFlowApiFactory applicationFactory)
    : IClassFixture<SupportFlowApiFactory>
{
    private const string OpenApiDocumentPath = "/openapi/v1.json";

    [Fact]
    public async Task GetOpenApi_InDevelopment_DescribesCreateOrganizationEndpoint()
    {
        // Arrange
        using var httpClient = applicationFactory.CreateClient();

        // Act
        using var response = await httpClient.GetAsync(OpenApiDocumentPath);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(MediaTypeNames.Application.Json, response.Content.Headers.ContentType?.MediaType);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(responseStream);

        var root = document.RootElement;
        var openApiVersion = root.GetProperty("openapi").GetString();

        Assert.NotNull(openApiVersion);
        Assert.StartsWith("3.1.", openApiVersion);

        var operation = root
            .GetProperty("paths")
            .GetProperty("/api/organizations")
            .GetProperty("post");

        Assert.Equal("CreateOrganization", operation.GetProperty("operationId").GetString());
        Assert.Contains(
            operation.GetProperty("tags").EnumerateArray(),
            tag => tag.GetString() == "Organizations");

        var requestSchemaReference = operation
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty(MediaTypeNames.Application.Json)
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();

        Assert.Equal("#/components/schemas/CreateOrganizationRequest", requestSchemaReference);

        var responses = operation.GetProperty("responses");

        Assert.True(responses.TryGetProperty("201", out var createdResponse));
        Assert.True(responses.TryGetProperty("400", out var validationProblemResponse));

        Assert.Equal(
            "#/components/schemas/CreateOrganizationResponse",
            GetResponseSchemaReference(createdResponse, MediaTypeNames.Application.Json));
        Assert.Equal(
            "#/components/schemas/HttpValidationProblemDetails",
            GetResponseSchemaReference(validationProblemResponse, MediaTypeNames.Application.ProblemJson));
    }

    [Fact]
    public async Task GetOpenApi_InProduction_ReturnsNotFound()
    {
        // Arrange
        await using var productionApplicationFactory = applicationFactory.WithWebHostBuilder(builder =>
            builder.UseEnvironment(Environments.Production));
        using var httpClient = productionApplicationFactory.CreateClient();

        // Act
        using var response = await httpClient.GetAsync(OpenApiDocumentPath);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static string? GetResponseSchemaReference(JsonElement response, string mediaType)
    {
        return response
            .GetProperty("content")
            .GetProperty(mediaType)
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
    }
}
