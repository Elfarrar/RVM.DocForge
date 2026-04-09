using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.DocForge.Domain.Entities;

namespace RVM.DocForge.Infrastructure.Data.Configurations;

public class ProjectSnapshotConfiguration : IEntityTypeConfiguration<ProjectSnapshot>
{
    public void Configure(EntityTypeBuilder<ProjectSnapshot> builder)
    {
        builder.ToTable("project_snapshots");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.GitCommitHash).HasMaxLength(40);
        builder.Property(s => s.GitBranch).HasMaxLength(100);

        builder.HasIndex(s => s.DocumentationProjectId);
        builder.HasIndex(s => s.AnalyzedAt);

        builder.HasMany(s => s.Endpoints).WithOne(e => e.ProjectSnapshot)
            .HasForeignKey(e => e.ProjectSnapshotId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Entities).WithOne(e => e.ProjectSnapshot)
            .HasForeignKey(e => e.ProjectSnapshotId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(s => s.Services).WithOne(s2 => s2.ProjectSnapshot)
            .HasForeignKey(s2 => s2.ProjectSnapshotId).OnDelete(DeleteBehavior.Cascade);
    }
}
