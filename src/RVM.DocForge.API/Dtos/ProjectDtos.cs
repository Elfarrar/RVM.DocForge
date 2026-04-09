using RVM.DocForge.Domain.Enums;

namespace RVM.DocForge.API.DTOs;

public record CreateProjectRequest(
    string Name,
    string? RepositoryPath,
    string? SolutionFile,
    string? Description,
    string? GitRemoteUrl);

public record UpdateProjectRequest(
    string? Name,
    string? RepositoryPath,
    string? SolutionFile,
    string? Description,
    string? GitRemoteUrl);

public record ProjectResponse(
    Guid Id,
    string Name,
    string RepositoryPath,
    string? SolutionFile,
    string? Description,
    string? GitRemoteUrl,
    DocumentationStatus Status,
    DateTime CreatedAt,
    DateTime? LastAnalyzedAt,
    int SnapshotCount,
    int DocumentCount);
