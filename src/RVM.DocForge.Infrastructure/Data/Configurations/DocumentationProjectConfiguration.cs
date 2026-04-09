using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.Infrastructure.Data.Configurations;

public class DocumentationProjectConfiguration : IEntityTypeConfiguration<DocumentationProject>
{
    public void Configure(EntityTypeBuilder<DocumentationProject> builder)
    {
        builder.ToTable("documentation_projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.RepositoryPath).IsRequired().HasMaxLength(500);
        builder.Property(p => p.SolutionFile).HasMaxLength(500);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.GitRemoteUrl).HasMaxLength(500);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(DocumentationStatus.Pending);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Name);

        builder.HasMany(p => p.Snapshots).WithOne(s => s.DocumentationProject)
            .HasForeignKey(s => s.DocumentationProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Documents).WithOne(d => d.DocumentationProject)
            .HasForeignKey(d => d.DocumentationProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}
