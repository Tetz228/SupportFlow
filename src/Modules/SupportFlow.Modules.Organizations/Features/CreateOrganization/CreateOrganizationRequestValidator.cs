using FluentValidation;
using SupportFlow.Modules.Organizations.Domain;

namespace SupportFlow.Modules.Organizations.Features.CreateOrganization;

internal sealed class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Organization name is required.")
            .Must(name => !name!.Contains('\0'))
            .WithMessage("Organization name cannot contain NUL characters.")
            .Must(name => name!.Trim().Length <= Organization.MaxNameLength)
            .WithMessage($"Organization name cannot exceed {Organization.MaxNameLength} characters.");
    }
}
