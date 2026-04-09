using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.Domain.Entities;

public class DocumentationProject
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string RepositoryPath { get; set; } = string.Empty;
    public string? SolutionFile { get; set; }
    public string? Description { get; set; }
    public string? GitRemoteUrl { get; set; }
    public DocumentationStatus Status { get; set; } = DocumentationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAnalyzedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProjectSnapshot> Snapshots { get; set; } = [];
    public ICollection<GeneratedDocument> Documents { get; set; } = [];
}
