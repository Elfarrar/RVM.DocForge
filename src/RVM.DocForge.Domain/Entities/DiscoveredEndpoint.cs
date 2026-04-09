namespace RVM.DocForge.Domain.Entities;

public class DiscoveredEndpoint
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ProjectSnapshotId { get; set; }
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string? RequestBodyType { get; set; }
    public string? ResponseType { get; set; }
    public string? Summary { get; set; }
    public string? ParametersJson { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public ProjectSnapshot ProjectSnapshot { get; set; } = null!;
}
