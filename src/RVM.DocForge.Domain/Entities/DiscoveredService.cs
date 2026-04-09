namespace RVM.DocForge.Domain.Entities;

public class DiscoveredService
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProjectSnapshotId { get; set; }
    public string InterfaceName { get; set; } = string.Empty;
    public string? ImplementationName { get; set; }
    public string? Summary { get; set; }
    public string MethodSignaturesJson { get; set; } = "[]";
    public string? Lifetime { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public ProjectSnapshot ProjectSnapshot { get; set; } = null!;
}
