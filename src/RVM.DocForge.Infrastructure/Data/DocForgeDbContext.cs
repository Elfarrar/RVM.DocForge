using Microsoft.EntityFrameworkCore;
using RVM.DocForge.Domain.Entities;

namespace RVM.DocForge.Infrastructure.Data;

public class DocForgeDbContext(DbContextOptions<DocForgeDbContext> options) : DbContext(options)
{
    public DbSet<DocumentationProject> DocumentationProjects => Set<DocumentationProject>();
    public DbSet<ProjectSnapshot> ProjectSnapshots => Set<ProjectSnapshot>();
    public DbSet<DiscoveredEndpoint> DiscoveredEndpoints => Set<DiscoveredEndpoint>();
    public DbSet<DiscoveredEntity> DiscoveredEntities => Set<DiscoveredEntity>();
    public DbSet<DiscoveredService> DiscoveredServices => Set<DiscoveredService>();
    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocForgeDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<DocumentationProject>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
