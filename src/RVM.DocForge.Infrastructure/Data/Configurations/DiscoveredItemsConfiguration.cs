using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.Infrastructure.Data.Configurations;

public class DiscoveredEndpointConfiguration : IEntityTypeConfiguration<DiscoveredEndpoint>
{
    public void Configure(EntityTypeBuilder<DiscoveredEndpoint> builder)
    {
        builder.ToTable("discovered_endpoints");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ControllerName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ActionName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.HttpMethod).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Route).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ProjectName).HasMaxLength(200);
        builder.HasIndex(e => e.ProjectSnapshotId);
    }
}

public class DiscoveredEntityConfiguration : IEntityTypeConfiguration<DiscoveredEntity>
{
    public void Configure(EntityTypeBuilder<DiscoveredEntity> builder)
    {
        builder.ToTable("discovered_entities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Namespace).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Kind).IsRequired().HasMaxLength(20);
        builder.Property(e => e.ProjectName).HasMaxLength(200);
        builder.HasIndex(e => e.ProjectSnapshotId);
    }
}

public class DiscoveredServiceConfiguration : IEntityTypeConfiguration<DiscoveredService>
{
    public void Configure(EntityTypeBuilder<DiscoveredService> builder)
    {
        builder.ToTable("discovered_services");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.InterfaceName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ImplementationName).HasMaxLength(200);
        builder.Property(s => s.Lifetime).HasMaxLength(20);
        builder.Property(s => s.ProjectName).HasMaxLength(200);
        builder.HasIndex(s => s.ProjectSnapshotId);
    }
}

public class GeneratedDocumentConfiguration : IEntityTypeConfiguration<GeneratedDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedDocument> builder)
    {
        builder.ToTable("generated_documents");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Content).IsRequired();
        builder.Property(d => d.Format).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(OutputFormat.Markdown);
        builder.HasIndex(d => d.DocumentationProjectId);
        builder.HasIndex(d => d.Type);
    }
}
