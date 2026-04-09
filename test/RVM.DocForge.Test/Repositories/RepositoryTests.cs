using Microsoft.EntityFrameworkCore;
using RVM.DocForge.Domain.Entities;
using RVM.DocForge.Domain.Enums;
using RVM.DocForge.Infrastructure.Data;
using RVM.DocForge.Infrastructure.Repositories;

namespace RVM.DocForge.Test.Repositories;

public class RepositoryTests
{
    private static DocForgeDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<DocForgeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DocForgeDbContext(options);
    }

    [Fact]
    public async Task DocumentationProjectRepository_CRUD()
    {
        using var db = CreateDb();
        var repo = new DocumentationProjectRepository(db);

        var project = new DocumentationProject
        {
            Name = "TestProject",
            RepositoryPath = "/test/path",
            Description = "A test project"
        };

        await repo.AddAsync(project);
        var all = await repo.GetAllAsync();
        Assert.Single(all);

        var found = await repo.GetByIdAsync(project.Id);
        Assert.NotNull(found);
        Assert.Equal("TestProject", found.Name);

        found.Name = "Updated";
        await repo.UpdateAsync(found);
        var updated = await repo.GetByIdAsync(project.Id);
        Assert.Equal("Updated", updated!.Name);

        await repo.DeleteAsync(project.Id);
        var deleted = await repo.GetByIdAsync(project.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task ProjectSnapshotRepository_AddAndQuery()
    {
        using var db = CreateDb();
        var projectRepo = new DocumentationProjectRepository(db);
        var snapshotRepo = new ProjectSnapshotRepository(db);

        var project = new DocumentationProject
        {
            Name = "TestProject",
            RepositoryPath = "/test/path"
        };
        await projectRepo.AddAsync(project);

        var snapshot = new ProjectSnapshot
        {
            DocumentationProjectId = project.Id,
            ProjectCount = 3,
            TotalEndpoints = 10,
            TotalEntities = 5,
            TotalServices = 4
        };
        await snapshotRepo.AddAsync(snapshot);

        var latest = await snapshotRepo.GetLatestByProjectIdAsync(project.Id);
        Assert.NotNull(latest);
        Assert.Equal(3, latest.ProjectCount);

        var all = await snapshotRepo.GetByProjectIdAsync(project.Id);
        Assert.Single(all);
    }

    [Fact]
    public async Task GeneratedDocumentRepository_AddAndQuery()
    {
        using var db = CreateDb();
        var projectRepo = new DocumentationProjectRepository(db);
        var docRepo = new GeneratedDocumentRepository(db);

        var project = new DocumentationProject
        {
            Name = "TestProject",
            RepositoryPath = "/test/path"
        };
        await projectRepo.AddAsync(project);

        var doc = new GeneratedDocument
        {
            DocumentationProjectId = project.Id,
            Type = DocumentType.Readme,
            Title = "README",
            Content = "# Test"
        };
        await docRepo.AddAsync(doc);

        var docs = await docRepo.GetByProjectIdAsync(project.Id);
        Assert.Single(docs);

        var found = await docRepo.GetByIdAsync(doc.Id);
        Assert.NotNull(found);
        Assert.Equal("# Test", found.Content);

        await docRepo.DeleteAsync(doc.Id);
        var deleted = await docRepo.GetByIdAsync(doc.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task GeneratedDocumentRepository_DeleteByProjectId()
    {
        using var db = CreateDb();
        var projectRepo = new DocumentationProjectRepository(db);
        var docRepo = new GeneratedDocumentRepository(db);

        var project = new DocumentationProject
        {
            Name = "TestProject",
            RepositoryPath = "/test/path"
        };
        await projectRepo.AddAsync(project);

        await docRepo.AddAsync(new GeneratedDocument
        {
            DocumentationProjectId = project.Id,
            Type = DocumentType.Readme,
            Title = "README",
            Content = "# Test 1"
        });
        await docRepo.AddAsync(new GeneratedDocument
        {
            DocumentationProjectId = project.Id,
            Type = DocumentType.ApiReference,
            Title = "API Ref",
            Content = "# API"
        });

        var docs = await docRepo.GetByProjectIdAsync(project.Id);
        Assert.Equal(2, docs.Count);

        await docRepo.DeleteByProjectIdAsync(project.Id);
        docs = await docRepo.GetByProjectIdAsync(project.Id);
        Assert.Empty(docs);
    }
}
