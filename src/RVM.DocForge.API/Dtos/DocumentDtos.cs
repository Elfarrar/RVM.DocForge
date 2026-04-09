using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.API.DTOs;

public record GenerateDocumentsRequest(
    Guid ProjectId,
    List<DocumentType> DocumentTypes);

public record DocumentResponse(
    Guid Id,
    Guid DocumentationProjectId,
    Guid? ProjectSnapshotId,
    DocumentType Type,
    string Title,
    OutputFormat Format,
    DateTime GeneratedAt);

public record DocumentContentResponse(
    Guid Id,
    string Title,
    DocumentType Type,
    string Content,
    OutputFormat Format,
    DateTime GeneratedAt);
