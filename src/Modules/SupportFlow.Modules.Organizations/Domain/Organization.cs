namespace SupportFlow.Modules.Organizations.Domain;

internal sealed class Organization
{
    internal const int MaxNameLength = 200;

    private Organization(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public static Organization Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Organization name cannot exceed {MaxNameLength} characters.", nameof(name));
        }

        return new Organization(Guid.CreateVersion7(), normalizedName);
    }
}
