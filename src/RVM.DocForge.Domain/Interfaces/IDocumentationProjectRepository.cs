using RVM.DocForge.Domain.Entities;

namespace RVM.DocForge.Domain.Interfaces;

public interface IDocumentationProjectRepository
{
    Task<List<DocumentationProject>> GetAllAsync(CancellationToken ct = default);
    Task<DocumentationProject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DocumentationProject?> GetByIdWithSnapshotsAsync(Guid id, CancellationToken ct = default);
    Task<DocumentationProject?> GetByIdWithDocumentsAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DocumentationProject project, CancellationToken ct = default);
    Task UpdateAsync(DocumentationProject project, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
