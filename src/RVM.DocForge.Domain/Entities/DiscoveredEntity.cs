namespace RVM.DocForge.Domain.Entities;

public class DiscoveredEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProjectSnapshotId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // class, record, struct, enum
    public string? Summary { get; set; }
    public string PropertiesJson { get; set; } = "[]";
    public string? BaseType { get; set; }
    public string? ImplementedInterfaces { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public ProjectSnapshot ProjectSnapshot { get; set; } = null!;
}
