using System.Text.Json;
using RVM.DocForge.API.Services.Generators;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Enums;
using RVM.DocForge.Domain.Interfaces;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services;

public class DocumentationOrchestrator(
    RepositoryAnalyzerService analyzer,
    IDocumentationProjectRepository projectRepo,
    IProjectSnapshotRepository snapshotRepo,
    IGeneratedDocumentRepository documentRepo,
    ILogger<DocumentationOrchestrator> logger)
{
    private static readonly Dictionary<DocumentType, IDocumentGenerator> Generators = new()
    {
        [DocumentType.Readme] = new ReadmeGenerator(),
        [DocumentType.ApiReference] = new ApiReferenceGenerator(),
        [DocumentType.EntitySchema] = new EntitySchemaGenerator(),
        [DocumentType.ArchitectureOverview] = new ArchitectureOverviewGenerator(),
        [DocumentType.DependencyGraph] = new DependencyGraphGenerator(),
        [DocumentType.ServiceCatalog] = new ServiceCatalogGenerator(),
        [DocumentType.FullDocumentation] = new FullDocumentationGenerator(),
    };

    public async Task<ProjectSnapshot> AnalyzeProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var project = await projectRepo.GetByIdAsync(projectId, ct)
            ?? throw new InvalidOperationException($"Project {projectId} not found.");

        logger.LogInformation("Starting analysis for project {Name} at {Path}", project.Name, project.RepositoryPath);

        project.Status = DocumentationStatus.Analyzing;
        await projectRepo.UpdateAsync(project, ct);

        try
        {
            var result = await analyzer.AnalyzeAsync(project.RepositoryPath, ct);

            var snapshot = new ProjectSnapshot
            {
                DocumentationProjectId = projectId,
                ProjectCount = result.Projects.Count,
                TotalEndpoints = result.Endpoints.Count,
                TotalEntities = result.Entities.Count,
                TotalServices = result.Services.Count,
                TotalClasses = result.Entities.Count(e => e.Kind == "class"),
            };

            // Map discovered items to snapshot entities
            foreach (var ep in result.Endpoints)
            {
                snapshot.Endpoints.Add(new DiscoveredEndpoint
                {
                    ProjectSnapshotId = snapshot.Id,
                    ControllerName = ep.Controller,
                    ActionName = ep.Action,
                    HttpMethod = ep.HttpMethod,
                    Route = ep.Route,
                    RequestBodyType = ep.RequestType,
                    ResponseType = ep.ResponseType,
                    Summary = ep.Summary,
                    ParametersJson = JsonSerializer.Serialize(ep.Parameters),
                    ProjectName = ep.ProjectName,
                });
            }

            foreach (var entity in result.Entities)
            {
                snapshot.Entities.Add(new DiscoveredEntity
                {
                    ProjectSnapshotId = snapshot.Id,
                    Name = entity.Name,
                    Namespace = entity.Namespace,
                    Kind = entity.Kind,
                    Summary = entity.Summary,
                    PropertiesJson = JsonSerializer.Serialize(entity.Properties),
                    BaseType = entity.BaseType,
                    ImplementedInterfaces = entity.Interfaces.Count > 0
                        ? string.Join(", ", entity.Interfaces) : null,
                    ProjectName = entity.ProjectName,
                });
            }

            foreach (var svc in result.Services)
            {
                snapshot.Services.Add(new DiscoveredService
                {
                    ProjectSnapshotId = snapshot.Id,
                    InterfaceName = svc.InterfaceName,
                    ImplementationName = svc.ImplementationName,
                    Summary = svc.Summary,
                    MethodSignaturesJson = JsonSerializer.Serialize(svc.MethodSignatures),
                    Lifetime = svc.Lifetime,
                    ProjectName = svc.ProjectName,
                });
            }

            await snapshotRepo.AddAsync(snapshot, ct);

            project.Status = DocumentationStatus.Analyzed;
            project.LastAnalyzedAt = DateTime.UtcNow;
            await projectRepo.UpdateAsync(project, ct);

            logger.LogInformation("Analysis complete for project {Name}: {Endpoints} endpoints, {Entities} entities, {Services} services",
                project.Name, result.Endpoints.Count, result.Entities.Count, result.Services.Count);

            return snapshot;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analysis failed for project {Name}", project.Name);
            project.Status = DocumentationStatus.Failed;
            await projectRepo.UpdateAsync(project, ct);
            throw;
        }
    }

    public async Task<List<GeneratedDocument>> GenerateDocumentsAsync(
        Guid projectId,
        List<DocumentType> documentTypes,
        CancellationToken ct = default)
    {
        var project = await projectRepo.GetByIdAsync(projectId, ct)
            ?? throw new InvalidOperationException($"Project {projectId} not found.");

        var snapshot = await snapshotRepo.GetLatestByProjectIdAsync(projectId, ct)
            ?? throw new InvalidOperationException($"No analysis snapshot found for project {project.Name}. Run analysis first.");

        logger.LogInformation("Generating {Count} document(s) for project {Name}", documentTypes.Count, project.Name);

        project.Status = DocumentationStatus.Generating;
        await projectRepo.UpdateAsync(project, ct);

        try
        {
            // Rebuild analysis result from snapshot
            var analysis = await analyzer.AnalyzeAsync(project.RepositoryPath, ct);
            var documents = new List<GeneratedDocument>();

            foreach (var docType in documentTypes)
            {
                ct.ThrowIfCancellationRequested();

                if (!Generators.TryGetValue(docType, out var generator))
                {
                    logger.LogWarning("No generator found for document type {Type}", docType);
                    continue;
                }

                var content = generator.Generate(analysis, project.Name);

                var doc = new GeneratedDocument
                {
                    DocumentationProjectId = projectId,
                    ProjectSnapshotId = snapshot.Id,
                    Type = docType,
                    Title = $"{project.Name} — {docType}",
                    Content = content,
                    Format = OutputFormat.Markdown,
                };

                await documentRepo.AddAsync(doc, ct);
                documents.Add(doc);

                logger.LogInformation("Generated {Type} document for project {Name}", docType, project.Name);
            }

            project.Status = DocumentationStatus.Ready;
            await projectRepo.UpdateAsync(project, ct);

            return documents;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Document generation failed for project {Name}", project.Name);
            project.Status = DocumentationStatus.Failed;
            await projectRepo.UpdateAsync(project, ct);
            throw;
        }
    }
}
