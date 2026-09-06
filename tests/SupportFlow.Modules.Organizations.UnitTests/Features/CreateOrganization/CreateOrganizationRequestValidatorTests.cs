using FluentValidation.TestHelper;
using SupportFlow.Modules.Organizations.Domain;
using SupportFlow.Modules.Organizations.Features.CreateOrganization;

namespace SupportFlow.Modules.Organizations.UnitTests.Features.CreateOrganization;

public sealed class CreateOrganizationRequestValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WithValidName_HasNoValidationErrors()
    {
        // Arrange
        var request = new CreateOrganizationRequest("Acme");
        var validator = new CreateOrganizationRequestValidator();

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(createOrganizationRequest => createOrganizationRequest.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ValidateAsync_WithNullOrWhiteSpaceName_HasRequiredValidationError(string? name)
    {
        // Arrange
        var request = new CreateOrganizationRequest(name);
        var validator = new CreateOrganizationRequestValidator();

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(createOrganizationRequest => createOrganizationRequest.Name)
            .WithErrorMessage("Organization name is required.")
            .Only();
    }

    [Fact]
    public async Task ValidateAsync_WithNameAtMaximumLength_HasNoValidationErrors()
    {
        // Arrange
        var name = new string('a', Organization.MaxNameLength);
        var request = new CreateOrganizationRequest(name);
        var validator = new CreateOrganizationRequestValidator();

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(createOrganizationRequest => createOrganizationRequest.Name);
    }

    [Fact]
    public async Task ValidateAsync_WithNameExceedingMaximumLength_HasMaximumLengthValidationError()
    {
        // Arrange
        var name = new string('a', Organization.MaxNameLength + 1);
        var request = new CreateOrganizationRequest(name);
        var validator = new CreateOrganizationRequestValidator();

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(createOrganizationRequest => createOrganizationRequest.Name)
            .WithErrorMessage($"Organization name cannot exceed {Organization.MaxNameLength} characters.")
            .Only();
    }

    [Fact]
    public async Task ValidateAsync_WithNameAtMaximumLengthAndSurroundingWhitespace_HasNoValidationErrors()
    {
        // Arrange
        var name = $"     {new string('a', Organization.MaxNameLength)}     ";
        var request = new CreateOrganizationRequest(name);
        var validator = new CreateOrganizationRequestValidator();

        // Act
        var result = await validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(createOrganizationRequest => createOrganizationRequest.Name);
    }
}
