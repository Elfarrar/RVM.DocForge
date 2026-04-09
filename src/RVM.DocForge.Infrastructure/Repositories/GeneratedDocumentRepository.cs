using Microsoft.EntityFrameworkCore;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Interfaces;
using RVM.DocForge.Infrastructure.Data;

namespace RVM.DocForge.Infrastructure.Repositories;

public class GeneratedDocumentRepository(DocForgeDbContext db) : IGeneratedDocumentRepository
{
    public Task<List<GeneratedDocument>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default)
        => db.GeneratedDocuments
            .Where(d => d.DocumentationProjectId == projectId)
            .OrderByDescending(d => d.GeneratedAt)
            .ToListAsync(ct);

    public Task<GeneratedDocument?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.GeneratedDocuments.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddAsync(GeneratedDocument document, CancellationToken ct = default)
    {
        db.GeneratedDocuments.Add(document);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var doc = await db.GeneratedDocuments.FindAsync([id], ct);
        if (doc is not null)
        {
            db.GeneratedDocuments.Remove(doc);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteByProjectIdAsync(Guid projectId, CancellationToken ct = default)
    {
        var docs = await db.GeneratedDocuments
            .Where(d => d.DocumentationProjectId == projectId)
            .ToListAsync(ct);
        db.GeneratedDocuments.RemoveRange(docs);
        await db.SaveChangesAsync(ct);
    }
}
