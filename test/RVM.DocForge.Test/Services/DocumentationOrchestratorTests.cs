using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RVM.DocForge.API.Services;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Enums;
using RVM.DocForge.Domain.Interfaces;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.Test.Services;

/// <summary>
/// Testa o DocumentationOrchestrator com repositorios mockados e
/// um RepositoryAnalyzerService real sobre diretorios temporarios.
/// </summary>
public class DocumentationOrchestratorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IDocumentationProjectRepository> _projectRepo = new();
    private readonly Mock<IProjectSnapshotRepository> _snapshotRepo = new();
    private readonly Mock<IGeneratedDocumentRepository> _documentRepo = new();
    private readonly RepositoryAnalyzerService _analyzer;
    private readonly DocumentationOrchestrator _orchestrator;

    public DocumentationOrchestratorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docforge-orch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _analyzer = new RepositoryAnalyzerService(NullLogger<RepositoryAnalyzerService>.Instance);
        _orchestrator = new DocumentationOrchestrator(
            _analyzer,
            _projectRepo.Object,
            _snapshotRepo.Object,
            _documentRepo.Object,
            NullLogger<DocumentationOrchestrator>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DocumentationProject CreateProject(string name = "TestProject") => new()
    {
        Name = name,
        RepositoryPath = _tempDir,
        Status = DocumentationStatus.Pending,
    };

    // -------------------------------------------------------------------------
    // AnalyzeProjectAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeProjectAsync_ProjectNotFound_ThrowsInvalidOperation()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentationProject?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.AnalyzeProjectAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AnalyzeProjectAsync_ValidProject_ChangesStatusToAnalyzed()
    {
        var project = CreateProject();
        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        await _orchestrator.AnalyzeProjectAsync(project.Id);

        Assert.Equal(DocumentationStatus.Analyzed, project.Status);
        Assert.NotNull(project.LastAnalyzedAt);
    }

    [Fact]
    public async Task AnalyzeProjectAsync_ValidProject_PersistsSnapshot()
    {
        var project = CreateProject();
        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        await _orchestrator.AnalyzeProjectAsync(project.Id);

        _snapshotRepo.Verify(r => r.AddAsync(It.IsAny<ProjectSnapshot>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnalyzeProjectAsync_ValidProject_UpdatesProjectTwice()
    {
        // Once to set Analyzing, once to set Analyzed
        var project = CreateProject();
        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        await _orchestrator.AnalyzeProjectAsync(project.Id);

        _projectRepo.Verify(r => r.UpdateAsync(It.IsAny<DocumentationProject>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AnalyzeProjectAsync_WithCsproj_SnapshotHasProjectCount()
    {
        File.WriteAllText(Path.Combine(_tempDir, "App.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        var project = CreateProject();
        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        ProjectSnapshot? capturedSnapshot = null;
        _snapshotRepo.Setup(r => r.AddAsync(It.IsAny<ProjectSnapshot>(), It.IsAny<CancellationToken>()))
            .Callback<ProjectSnapshot, CancellationToken>((s, _) => capturedSnapshot = s);

        await _orchestrator.AnalyzeProjectAsync(project.Id);

        Assert.NotNull(capturedSnapshot);
        Assert.Equal(1, capturedSnapshot!.ProjectCount);
    }

    [Fact]
    public async Task AnalyzeProjectAsync_OnException_SetsStatusFailed()
    {
        var project = CreateProject();
        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Make snapshotRepo throw to trigger the catch block
        _snapshotRepo.Setup(r => r.AddAsync(It.IsAny<ProjectSnapshot>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.AnalyzeProjectAsync(project.Id));

        Assert.Equal(DocumentationStatus.Failed, project.Status);
    }

    // -------------------------------------------------------------------------
    // GenerateDocumentsAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GenerateDocumentsAsync_ProjectNotFound_ThrowsInvalidOperation()
    {
        _projectRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentationProject?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.GenerateDocumentsAsync(Guid.NewGuid(), [DocumentType.Readme]));
    }

    [Fact]
    public async Task GenerateDocumentsAsync_NoSnapshot_ThrowsInvalidOperation()
    {
        var project = CreateProject();
        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _snapshotRepo.Setup(r => r.GetLatestByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectSnapshot?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.GenerateDocumentsAsync(project.Id, [DocumentType.Readme]));
    }

    [Fact]
    public async Task GenerateDocumentsAsync_WithReadme_GeneratesDocument()
    {
        var project = CreateProject();
        var snapshot = new ProjectSnapshot { DocumentationProjectId = project.Id };

        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _snapshotRepo.Setup(r => r.GetLatestByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var docs = await _orchestrator.GenerateDocumentsAsync(project.Id, [DocumentType.Readme]);

        Assert.Single(docs);
        Assert.Equal(DocumentType.Readme, docs[0].Type);
        Assert.NotEmpty(docs[0].Content);
    }

    [Fact]
    public async Task GenerateDocumentsAsync_MultipleTypes_GeneratesAll()
    {
        var project = CreateProject();
        var snapshot = new ProjectSnapshot { DocumentationProjectId = project.Id };

        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _snapshotRepo.Setup(r => r.GetLatestByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var types = new List<DocumentType>
        {
            DocumentType.Readme,
            DocumentType.ApiReference,
            DocumentType.ArchitectureOverview,
        };

        var docs = await _orchestrator.GenerateDocumentsAsync(project.Id, types);

        Assert.Equal(3, docs.Count);
        _documentRepo.Verify(r => r.AddAsync(It.IsAny<GeneratedDocument>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task GenerateDocumentsAsync_SetsStatusToReady_OnSuccess()
    {
        var project = CreateProject();
        var snapshot = new ProjectSnapshot { DocumentationProjectId = project.Id };

        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _snapshotRepo.Setup(r => r.GetLatestByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        await _orchestrator.GenerateDocumentsAsync(project.Id, [DocumentType.Readme]);

        Assert.Equal(DocumentationStatus.Ready, project.Status);
    }

    [Fact]
    public async Task GenerateDocumentsAsync_DocumentTitleContainsProjectName()
    {
        var project = CreateProject("CoolProject");
        var snapshot = new ProjectSnapshot { DocumentationProjectId = project.Id };

        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _snapshotRepo.Setup(r => r.GetLatestByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        GeneratedDocument? captured = null;
        _documentRepo.Setup(r => r.AddAsync(It.IsAny<GeneratedDocument>(), It.IsAny<CancellationToken>()))
            .Callback<GeneratedDocument, CancellationToken>((d, _) => captured = d);

        await _orchestrator.GenerateDocumentsAsync(project.Id, [DocumentType.Readme]);

        Assert.NotNull(captured);
        Assert.Contains("CoolProject", captured!.Title);
    }

    [Fact]
    public async Task GenerateDocumentsAsync_OnException_SetsStatusFailed()
    {
        var project = CreateProject();
        var snapshot = new ProjectSnapshot { DocumentationProjectId = project.Id };

        _projectRepo.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _snapshotRepo.Setup(r => r.GetLatestByProjectIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);
        _documentRepo.Setup(r => r.AddAsync(It.IsAny<GeneratedDocument>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.GenerateDocumentsAsync(project.Id, [DocumentType.Readme]));

        Assert.Equal(DocumentationStatus.Failed, project.Status);
    }
}
