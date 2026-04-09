using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.DocForge.API.DTOs;
using RVM.DocForge.API.Services;
using RVM.DocForge.Domain.Interfaces;

namespace RVM.DocForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController(
    DocumentationOrchestrator orchestrator,
    IGeneratedDocumentRepository documentRepo,
    ILogger<DocumentsController> logger) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<List<DocumentResponse>>> Generate(GenerateDocumentsRequest request, CancellationToken ct)
    {
        try
        {
            var docs = await orchestrator.GenerateDocumentsAsync(request.ProjectId, request.DocumentTypes, ct);
            logger.LogInformation("Generated {Count} document(s) for project {ProjectId}", docs.Count, request.ProjectId);

            return docs.Select(d => new DocumentResponse(
                d.Id, d.DocumentationProjectId, d.ProjectSnapshotId,
                d.Type, d.Title, d.Format, d.GeneratedAt)).ToList();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<List<DocumentResponse>>> GetByProject(Guid projectId, CancellationToken ct)
    {
        var docs = await documentRepo.GetByProjectIdAsync(projectId, ct);
        return docs.Select(d => new DocumentResponse(
            d.Id, d.DocumentationProjectId, d.ProjectSnapshotId,
            d.Type, d.Title, d.Format, d.GeneratedAt)).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentContentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var doc = await documentRepo.GetByIdAsync(id, ct);
        if (doc is null) return NotFound();

        return new DocumentContentResponse(doc.Id, doc.Title, doc.Type, doc.Content, doc.Format, doc.GeneratedAt);
    }

    [HttpGet("{id:guid}/raw")]
    public async Task<IActionResult> GetRaw(Guid id, CancellationToken ct)
    {
        var doc = await documentRepo.GetByIdAsync(id, ct);
        if (doc is null) return NotFound();

        return Content(doc.Content, "text/markdown");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var doc = await documentRepo.GetByIdAsync(id, ct);
        if (doc is null) return NotFound();

        await documentRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpDelete("project/{projectId:guid}")]
    public async Task<IActionResult> DeleteByProject(Guid projectId, CancellationToken ct)
    {
        await documentRepo.DeleteByProjectIdAsync(projectId, ct);
        return NoContent();
    }
}
