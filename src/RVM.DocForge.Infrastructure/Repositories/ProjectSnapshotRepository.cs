using Microsoft.EntityFrameworkCore;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Interfaces;
using RVM.DocForge.Infrastructure.Data;

namespace RVM.DocForge.Infrastructure.Repositories;

public class ProjectSnapshotRepository(DocForgeDbContext db) : IProjectSnapshotRepository
{
    public Task<ProjectSnapshot?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => db.ProjectSnapshots
            .Include(s => s.Endpoints)
            .Include(s => s.Entities)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<ProjectSnapshot?> GetLatestByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => db.ProjectSnapshots
            .Include(s => s.Endpoints)
            .Include(s => s.Entities)
            .Include(s => s.Services)
            .Where(s => s.DocumentationProjectId == projectId)
            .OrderByDescending(s => s.AnalyzedAt)
            .FirstOrDefaultAsync(ct);

    public Task<List<ProjectSnapshot>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => db.ProjectSnapshots
            .Where(s => s.DocumentationProjectId == projectId)
            .OrderByDescending(s => s.AnalyzedAt)
            .ToListAsync(ct);

    public async Task AddAsync(ProjectSnapshot snapshot, CancellationToken ct = default)
    {
        db.ProjectSnapshots.Add(snapshot);
        await db.SaveChangesAsync(ct);
    }
}
