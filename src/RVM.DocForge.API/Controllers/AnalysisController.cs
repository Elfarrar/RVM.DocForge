using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.DocForge.API.DTOs;
using RVM.DocForge.API.Services;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Interfaces;

namespace RVM.DocForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalysisController(
    DocumentationOrchestrator orchestrator,
    IProjectSnapshotRepository snapshotRepo,
    ILogger<AnalysisController> logger) : ControllerBase
{
    [HttpPost("{projectId:guid}")]
    public async Task<ActionResult<SnapshotResponse>> Analyze(Guid projectId, CancellationToken ct)
    {
        try
        {
            var snapshot = await orchestrator.AnalyzeProjectAsync(projectId, ct);
            logger.LogInformation("Analysis completed for project {ProjectId}, snapshot {SnapshotId}", projectId, snapshot.Id);
            return CreatedAtAction(nameof(GetSnapshot), new { id = snapshot.Id }, MapToResponse(snapshot));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("snapshots/{id:guid}")]
    public async Task<ActionResult<SnapshotDetailResponse>> GetSnapshot(Guid id, CancellationToken ct)
    {
        var snapshot = await snapshotRepo.GetByIdWithDetailsAsync(id, ct);
        if (snapshot is null) return NotFound();

        return MapToDetailResponse(snapshot);
    }

    [HttpGet("project/{projectId:guid}/snapshots")]
    public async Task<ActionResult<List<SnapshotResponse>>> GetSnapshots(Guid projectId, CancellationToken ct)
    {
        var snapshots = await snapshotRepo.GetByProjectIdAsync(projectId, ct);
        return snapshots.Select(MapToResponse).ToList();
    }

    private static SnapshotResponse MapToResponse(ProjectSnapshot s) => new(
        s.Id, s.DocumentationProjectId, s.GitCommitHash, s.GitBranch,
        s.ProjectCount, s.TotalClasses, s.TotalEndpoints, s.TotalEntities, s.TotalServices, s.AnalyzedAt);

    private static SnapshotDetailResponse MapToDetailResponse(ProjectSnapshot s) => new(
        s.Id, s.DocumentationProjectId, s.ProjectCount,
        s.TotalEndpoints, s.TotalEntities, s.TotalServices, s.AnalyzedAt,
        s.Endpoints.Select(e => new EndpointSummary(e.ControllerName, e.ActionName, e.HttpMethod, e.Route, e.Summary)).ToList(),
        s.Entities.Select(e => new EntitySummary(e.Name, e.Namespace, e.Kind, e.Summary, 0)).ToList(),
        s.Services.Select(sv => new ServiceSummary(sv.InterfaceName, sv.ImplementationName, sv.Lifetime, 0)).ToList());
}
