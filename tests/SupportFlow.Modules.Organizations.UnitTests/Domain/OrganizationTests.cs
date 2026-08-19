using SupportFlow.Modules.Organizations.Domain;

namespace SupportFlow.Modules.Organizations.UnitTests.Domain;

public sealed class OrganizationTests
{
    [Fact]
    public void Create_WithValidName_CreatesOrganization()
    {
        // Arrange
        const string name = "Test";

        // Act
        var organization = Organization.Create(name);

        // Assert
        Assert.NotEqual(Guid.Empty, organization.Id);

        Assert.Equal(name, organization.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("      ")]
    public void Create_WithNullOrWhiteSpaceName_ThrowsArgumentException(string? name)
    {
        // Act
        Action act = () => Organization.Create(name!);

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithSurroundingWhitespace_TrimsName()
    {
        // Arrange
        const string name = "  Test  ";
        const string expectedName = "Test";

        // Act
        var organization = Organization.Create(name);

        // Assert
        Assert.Equal(expectedName, organization.Name);
    }

    [Fact]
    public void Create_WithNameAtMaximumLength_CreatesOrganization()
    {
        // Arrange
        var name = new string('a', Organization.MaxNameLength);

        // Act
        var organization = Organization.Create(name);

        // Assert
        Assert.Equal(name, organization.Name);
    }

    [Fact]
    public void Create_WithNameExceedingMaximumLength_ThrowsArgumentException()
    {
        // Arrange
        var name = new string('a', Organization.MaxNameLength + 1);

        // Act
        Action act = () => Organization.Create(name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_WithValidName_GeneratesVersion7Id()
    {
        // Act
        var organization = Organization.Create("Test");

        // Assert
        Assert.Equal(7, organization.Id.Version);
    }
}
