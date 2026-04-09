namespace RVM.DocForge.Domain.Models;

public record RepositoryAnalysisResult(
    List<ProjectInfo> Projects,
    List<EndpointInfo> Endpoints,
    List<EntityInfo> Entities,
    List<ServiceInfo> Services,
    List<DependencyLink> Dependencies,
    ArchitectureInfo Architecture);

public record ProjectInfo(
    string Name,
    string Path,
    string TargetFramework,
    string OutputType,
    List<string> ProjectReferences,
    List<PackageInfo> Packages);

public record PackageInfo(string Name, string Version);

public record EndpointInfo(
    string Controller,
    string Action,
    string HttpMethod,
    string Route,
    string? RequestType,
    string? ResponseType,
    string? Summary,
    List<ParameterInfo> Parameters,
    string ProjectName);

public record ParameterInfo(string Name, string Type, string Source);

public record EntityInfo(
    string Name,
    string Namespace,
    string Kind,
    string? Summary,
    List<PropertyDetail> Properties,
    string? BaseType,
    List<string> Interfaces,
    string ProjectName);

public record PropertyDetail(
    string Name,
    string Type,
    bool IsNullable,
    string? Summary,
    string? DefaultValue);

public record ServiceInfo(
    string InterfaceName,
    string? ImplementationName,
    string? Summary,
    List<string> MethodSignatures,
    string? Lifetime,
    string ProjectName);

public record DependencyLink(string From, string To);

public record ArchitectureInfo(
    List<LayerInfo> Layers,
    List<string> Observations);

public record LayerInfo(string Name, List<string> Projects);
