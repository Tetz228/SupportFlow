using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportFlow.Modules.Organizations.Domain;

namespace SupportFlow.Modules.Organizations.Infrastructure.Persistence.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(organization => organization.Id)
            .HasName("pk_organizations");

        builder.Property(organization => organization.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(organization => organization.Name)
            .HasColumnName("name")
            .HasMaxLength(Organization.MaxNameLength)
            .IsRequired();
    }
}
