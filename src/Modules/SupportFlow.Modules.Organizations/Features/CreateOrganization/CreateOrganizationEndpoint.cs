using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using SupportFlow.Modules.Organizations.Domain;
using SupportFlow.Modules.Organizations.Infrastructure.Persistence;

namespace SupportFlow.Modules.Organizations.Features.CreateOrganization;

internal static class CreateOrganizationEndpoint
{
    internal static IEndpointRouteBuilder MapCreateOrganizationEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/", HandleAsync)
            .WithName("CreateOrganization")
            .Produces<CreateOrganizationResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return endpointRouteBuilder;
    }

    private static async Task<Results<Created<CreateOrganizationResponse>, ValidationProblem>> HandleAsync(
        CreateOrganizationRequest createOrganizationRequest,
        IValidator<CreateOrganizationRequest> validator,
        OrganizationsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(createOrganizationRequest, cancellationToken);
        if (!validationResult.IsValid)
        {
            return TypedResults.ValidationProblem(validationResult.ToDictionary());
        }

        var organization = Organization.Create(createOrganizationRequest.Name!);

        dbContext.Organizations.Add(organization);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new CreateOrganizationResponse(organization.Id, organization.Name);

        return TypedResults.Created($"/api/organizations/{organization.Id}", response);
    }
}
