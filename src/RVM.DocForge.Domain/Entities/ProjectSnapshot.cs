namespace RVM.DocForge.Domain.Entities;

public class ProjectSnapshot
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DocumentationProjectId { get; set; }
    public string? GitCommitHash { get; set; }
    public string? GitBranch { get; set; }
    public int ProjectCount { get; set; }
    public int TotalClasses { get; set; }
    public int TotalEndpoints { get; set; }
    public int TotalEntities { get; set; }
    public int TotalServices { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    public DocumentationProject DocumentationProject { get; set; } = null!;
    public ICollection<DiscoveredEndpoint> Endpoints { get; set; } = [];
    public ICollection<DiscoveredEntity> Entities { get; set; } = [];
    public ICollection<DiscoveredService> Services { get; set; } = [];
}
