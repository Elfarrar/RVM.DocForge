using RVM.DocForge.Domain.Entities;

namespace RVM.DocForge.Domain.Interfaces;

public interface IProjectSnapshotRepository
{
    Task<ProjectSnapshot?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<ProjectSnapshot?> GetLatestByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task<List<ProjectSnapshot>> GetByProjectIdAsync(Guid projectId, CancellationToken ct = default);
    Task AddAsync(ProjectSnapshot snapshot, CancellationToken ct = default);
}
