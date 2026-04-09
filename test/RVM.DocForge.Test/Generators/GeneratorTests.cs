using RVM.DocForge.API.Services.Generators;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.Test.Generators;

public class GeneratorTests
{
    private static RepositoryAnalysisResult CreateSampleAnalysis()
    {
        var projects = new List<ProjectInfo>
        {
            new("MyApp.API", "/src/MyApp.API/MyApp.API.csproj", "net10.0", "Web",
                ["MyApp.Domain", "MyApp.Infrastructure"],
                [new PackageInfo("Serilog", "10.0.0"), new PackageInfo("Npgsql", "9.0.0")]),
            new("MyApp.Domain", "/src/MyApp.Domain/MyApp.Domain.csproj", "net10.0", "Library",
                [], []),
            new("MyApp.Infrastructure", "/src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", "net10.0", "Library",
                ["MyApp.Domain"],
                [new PackageInfo("Npgsql.EntityFrameworkCore.PostgreSQL", "10.0.0")])
        };

        var endpoints = new List<EndpointInfo>
        {
            new("UsersController", "GetAll", "GET", "/api/users", null, "IActionResult",
                "Gets all users", [new ParameterInfo("filter", "string", "Query")], "MyApp.API"),
            new("UsersController", "Create", "POST", "/api/users", "CreateUserRequest", "IActionResult",
                "Creates a user", [new ParameterInfo("request", "CreateUserRequest", "Body")], "MyApp.API")
        };

        var entities = new List<EntityInfo>
        {
            new("User", "MyApp.Domain.Entities", "class", "Represents a user",
                [new PropertyDetail("Id", "Guid", false, null, null),
                 new PropertyDetail("Name", "string", false, null, "string.Empty"),
                 new PropertyDetail("Email", "string?", true, null, null)],
                null, [], "MyApp.Domain"),
            new("Status", "MyApp.Domain.Enums", "enum", null,
                [new PropertyDetail("Active", "enum", false, null, "0"),
                 new PropertyDetail("Inactive", "enum", false, null, "1")],
                null, [], "MyApp.Domain")
        };

        var services = new List<ServiceInfo>
        {
            new("IUserService", "UserService", "Manages user operations",
                ["Task<User> GetByIdAsync(Guid id)", "Task CreateAsync(User user)"],
                "Scoped", "MyApp.API")
        };

        var dependencies = new List<DependencyLink>
        {
            new("MyApp.API", "MyApp.Domain"),
            new("MyApp.API", "MyApp.Infrastructure"),
            new("MyApp.Infrastructure", "MyApp.Domain")
        };

        var architecture = new ArchitectureInfo(
            [new LayerInfo("Presentation", ["MyApp.API"]),
             new LayerInfo("Domain", ["MyApp.Domain"]),
             new LayerInfo("Infrastructure", ["MyApp.Infrastructure"])],
            ["Clean Architecture pattern detected."]);

        return new RepositoryAnalysisResult(projects, endpoints, entities, services, dependencies, architecture);
    }

    [Fact]
    public void ReadmeGenerator_Should_Produce_Valid_Markdown()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new ReadmeGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("# MyApp", result);
        Assert.Contains("## Projects", result);
        Assert.Contains("## API Endpoints", result);
        Assert.Contains("## Entities", result);
        Assert.Contains("GET", result);
        Assert.Contains("UsersController", result);
    }

    [Fact]
    public void ApiReferenceGenerator_Should_List_All_Endpoints()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new ApiReferenceGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("# API Reference", result);
        Assert.Contains("`GET /api/users`", result);
        Assert.Contains("`POST /api/users`", result);
        Assert.Contains("CreateUserRequest", result);
    }

    [Fact]
    public void EntitySchemaGenerator_Should_List_All_Entities()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new EntitySchemaGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("# Entity Schema", result);
        Assert.Contains("`User`", result);
        Assert.Contains("`Status`", result);
        Assert.Contains("Guid", result);
    }

    [Fact]
    public void ArchitectureOverviewGenerator_Should_Include_Mermaid()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new ArchitectureOverviewGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("# Architecture Overview", result);
        Assert.Contains("```mermaid", result);
        Assert.Contains("graph TD", result);
        Assert.Contains("Presentation", result);
    }

    [Fact]
    public void DependencyGraphGenerator_Should_List_Packages()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new DependencyGraphGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("# Dependency Graph", result);
        Assert.Contains("Serilog", result);
        Assert.Contains("Npgsql", result);
    }

    [Fact]
    public void ServiceCatalogGenerator_Should_List_Services()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new ServiceCatalogGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("# Service Catalog", result);
        Assert.Contains("IUserService", result);
        Assert.Contains("UserService", result);
        Assert.Contains("Scoped", result);
    }

    [Fact]
    public void FullDocumentationGenerator_Should_Include_All_Sections()
    {
        var analysis = CreateSampleAnalysis();
        var generator = new FullDocumentationGenerator();

        var result = generator.Generate(analysis, "MyApp");

        Assert.Contains("Table of Contents", result);
        Assert.Contains("# MyApp", result);
        Assert.Contains("# API Reference", result);
        Assert.Contains("# Entity Schema", result);
        Assert.Contains("# Architecture Overview", result);
        Assert.Contains("# Dependency Graph", result);
        Assert.Contains("# Service Catalog", result);
    }

    [Fact]
    public void Generators_Should_Handle_Empty_Analysis()
    {
        var empty = new RepositoryAnalysisResult([], [], [], [], [], new ArchitectureInfo([], []));

        Assert.NotEmpty(new ReadmeGenerator().Generate(empty, "Empty"));
        Assert.NotEmpty(new ApiReferenceGenerator().Generate(empty, "Empty"));
        Assert.NotEmpty(new EntitySchemaGenerator().Generate(empty, "Empty"));
        Assert.NotEmpty(new ArchitectureOverviewGenerator().Generate(empty, "Empty"));
        Assert.NotEmpty(new DependencyGraphGenerator().Generate(empty, "Empty"));
        Assert.NotEmpty(new ServiceCatalogGenerator().Generate(empty, "Empty"));
        Assert.NotEmpty(new FullDocumentationGenerator().Generate(empty, "Empty"));
    }
}
