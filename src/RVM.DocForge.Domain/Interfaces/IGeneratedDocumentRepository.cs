using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.Domain.Interfaces;

public interface IGeneratedDocumentRepository
{
    Task<List<GeneratedDocument>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<GeneratedDocument?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(GeneratedDocument document, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task DeleteByProjectIdAsync(Guid projectId, CancellationToken ct = default);
}
