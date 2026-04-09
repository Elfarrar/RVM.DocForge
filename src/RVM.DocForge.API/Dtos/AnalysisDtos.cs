namespace RVM.DocForge.API.DTOs;

public record SnapshotResponse(
    Guid Id,
    Guid DocumentationProjectId,
    string? GitCommitHash,
    string? GitBranch,
    int ProjectCount,
    int TotalClasses,
    int TotalEndpoints,
    int TotalEntities,
    int TotalServices,
    DateTime AnalyzedAt);

public record SnapshotDetailResponse(
    Guid Id,
    Guid DocumentationProjectId,
    int ProjectCount,
    int TotalEndpoints,
    int TotalEntities,
    int TotalServices,
    DateTime AnalyzedAt,
    List<EndpointSummary> Endpoints,
    List<EntitySummary> Entities,
    List<ServiceSummary> Services);

public record EndpointSummary(
    string Controller,
    string Action,
    string HttpMethod,
    string Route,
    string? Summary);

public record EntitySummary(
    string Name,
    string Namespace,
    string Kind,
    string? Summary,
    int PropertyCount);

public record ServiceSummary(
    string InterfaceName,
    string? ImplementationName,
    string? Lifetime,
    int MethodCount);
