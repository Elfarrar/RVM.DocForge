using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.DocForge.API.DTOs;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Interfaces;

namespace RVM.DocForge.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController(
    IDocumentationProjectRepository projectRepo,
    ILogger<ProjectsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> GetAll(CancellationToken ct)
    {
        var projects = await projectRepo.GetAllAsync(ct);
        return projects.Select(p => MapToResponse(p)).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> GetById(Guid id, CancellationToken ct)
    {
        var project = await projectRepo.GetByIdWithSnapshotsAsync(id, ct);
        if (project is null) return NotFound();

        var docProject = await projectRepo.GetByIdWithDocumentsAsync(id, ct);
        return MapToResponse(project, docProject?.Documents.Count ?? 0);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var project = new DocumentationProject
        {
            Name = request.Name,
            RepositoryPath = request.RepositoryPath,
            SolutionFile = request.SolutionFile,
            Description = request.Description,
            GitRemoteUrl = request.GitRemoteUrl,
        };

        await projectRepo.AddAsync(project, ct);
        logger.LogInformation("Created project {Name} with ID {Id}", project.Name, project.Id);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, MapToResponse(project));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid id, UpdateProjectRequest request, CancellationToken ct)
    {
        var project = await projectRepo.GetByIdAsync(id, ct);
        if (project is null) return NotFound();

        if (request.Name is not null) project.Name = request.Name;
        if (request.RepositoryPath is not null) project.RepositoryPath = request.RepositoryPath;
        if (request.SolutionFile is not null) project.SolutionFile = request.SolutionFile;
        if (request.Description is not null) project.Description = request.Description;
        if (request.GitRemoteUrl is not null) project.GitRemoteUrl = request.GitRemoteUrl;

        await projectRepo.UpdateAsync(project, ct);
        return MapToResponse(project);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var project = await projectRepo.GetByIdAsync(id, ct);
        if (project is null) return NotFound();

        await projectRepo.DeleteAsync(id, ct);
        logger.LogInformation("Deleted project {Name} with ID {Id}", project.Name, id);

        return NoContent();
    }

    private static ProjectResponse MapToResponse(DocumentationProject p, int? documentCount = null)
    {
        return new ProjectResponse(
            p.Id, p.Name, p.RepositoryPath, p.SolutionFile, p.Description, p.GitRemoteUrl,
            p.Status, p.CreatedAt, p.LastAnalyzedAt,
            p.Snapshots?.Count ?? 0,
            documentCount ?? p.Documents?.Count ?? 0);
    }
}
