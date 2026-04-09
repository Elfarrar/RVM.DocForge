using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.Domain.Entities;

public class GeneratedDocument
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid DocumentationProjectId { get; set; }
    public Guid? ProjectSnapshotId { get; set; }
    public DocumentType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public OutputFormat Format { get; set; } = OutputFormat.Markdown;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public DocumentationProject DocumentationProject { get; set; } = null!;
}
