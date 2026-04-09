using Microsoft.EntityFrameworkCore;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Interfaces;
using RVM.DocForge.Infrastructure.Data;

namespace RVM.DocForge.Infrastructure.Repositories;

public class DocumentationProjectRepository(DocForgeDbContext db) : IDocumentationProjectRepository
{
    public Task<List<DocumentationProject>> GetAllAsync(CancellationToken ct = default)
        => db.DocumentationProjects.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);

    public Task<DocumentationProject?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.DocumentationProjects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<DocumentationProject?> GetByIdWithSnapshotsAsync(Guid id, CancellationToken ct = default)
        => db.DocumentationProjects
            .Include(p => p.Snapshots.OrderByDescending(s => s.AnalyzedAt))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<DocumentationProject?> GetByIdWithDocumentsAsync(Guid id, CancellationToken ct = default)
        => db.DocumentationProjects
            .Include(p => p.Documents.OrderByDescending(d => d.GeneratedAt))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task AddAsync(DocumentationProject project, CancellationToken ct = default)
    {
        db.DocumentationProjects.Add(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DocumentationProject project, CancellationToken ct = default)
    {
        db.DocumentationProjects.Update(project);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await db.DocumentationProjects.FindAsync([id], ct);
        if (project is not null)
        {
            db.DocumentationProjects.Remove(project);
            await db.SaveChangesAsync(ct);
        }
    }
}
