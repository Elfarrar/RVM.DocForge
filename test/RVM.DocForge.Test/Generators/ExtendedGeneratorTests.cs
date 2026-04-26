using RVM.DocForge.API.Services.Generators;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.Test.Generators;

/// <summary>
/// Tests adicionais de geradores — cobre casos de borda e ramos nao testados.
/// </summary>
public class ExtendedGeneratorTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static RepositoryAnalysisResult EmptyAnalysis() =>
        new([], [], [], [], [], new ArchitectureInfo([], []));

    private static EndpointInfo MakeEndpoint(
        string controller = "TestController",
        string action = "Get",
        string method = "GET",
        string route = "/api/test",
        string? requestType = null,
        string? responseType = null,
        string? summary = null,
        List<ParameterInfo>? parameters = null) =>
        new(controller, action, method, route, requestType, responseType, summary,
            parameters ?? [], "MyApp.API");

    private static EntityInfo MakeEntity(
        string name = "User",
        string kind = "class",
        string? summary = null,
        List<PropertyDetail>? properties = null,
        string? baseType = null,
        List<string>? interfaces = null) =>
        new(name, "MyApp.Domain", kind, summary, properties ?? [], baseType, interfaces ?? [], "MyApp.Domain");

    private static ServiceInfo MakeService(string iface = "IUserService", string? impl = "UserService") =>
        new(iface, impl, "Manages users", ["Task<User> GetAsync(Guid id)"], "Scoped", "MyApp.API");

    // -------------------------------------------------------------------------
    // ApiReferenceGenerator
    // -------------------------------------------------------------------------

    [Fact]
    public void ApiReference_NoEndpoints_ShowsNoEndpointsMessage()
    {
        var gen = new ApiReferenceGenerator();
        var result = gen.Generate(EmptyAnalysis(), "Proj");

        Assert.Contains("No API endpoints discovered", result);
    }

    [Fact]
    public void ApiReference_WithSummary_ShowsSummary()
    {
        var ep = MakeEndpoint(summary: "Returns all items.");
        var analysis = new RepositoryAnalysisResult([], [ep], [], [], [], new ArchitectureInfo([], []));
        var gen = new ApiReferenceGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("Returns all items.", result);
    }

    [Fact]
    public void ApiReference_WithRequestAndResponseType_ShowsBoth()
    {
        var ep = MakeEndpoint(action: "Create", method: "POST", route: "/api/items",
            requestType: "CreateItemRequest", responseType: "ItemResponse");
        var analysis = new RepositoryAnalysisResult([], [ep], [], [], [], new ArchitectureInfo([], []));
        var gen = new ApiReferenceGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("CreateItemRequest", result);
        Assert.Contains("ItemResponse", result);
    }

    [Fact]
    public void ApiReference_WithParameters_ShowsParameterTable()
    {
        var ep = MakeEndpoint(parameters:
        [
            new ParameterInfo("id", "Guid", "Route"),
            new ParameterInfo("filter", "string", "Query"),
        ]);
        var analysis = new RepositoryAnalysisResult([], [ep], [], [], [], new ArchitectureInfo([], []));
        var gen = new ApiReferenceGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("| `id` |", result);
        Assert.Contains("| `filter` |", result);
        Assert.Contains("Route", result);
        Assert.Contains("Query", result);
    }

    [Fact]
    public void ApiReference_MultipleControllers_GroupedSeparately()
    {
        var ep1 = MakeEndpoint(controller: "UsersController", route: "/api/users");
        var ep2 = MakeEndpoint(controller: "OrdersController", route: "/api/orders");
        var analysis = new RepositoryAnalysisResult([], [ep1, ep2], [], [], [], new ArchitectureInfo([], []));
        var gen = new ApiReferenceGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("## UsersController", result);
        Assert.Contains("## OrdersController", result);
    }

    [Fact]
    public void ApiReference_CountLine_ReflectsCorrectNumbers()
    {
        var ep1 = MakeEndpoint(controller: "UsersController", action: "GetAll", route: "/api/users");
        var ep2 = MakeEndpoint(controller: "UsersController", action: "GetById", route: "/api/users/{id}");
        var analysis = new RepositoryAnalysisResult([], [ep1, ep2], [], [], [], new ArchitectureInfo([], []));
        var gen = new ApiReferenceGenerator();

        var result = gen.Generate(analysis, "Proj");

        // "2 endpoints across 1 controllers"
        Assert.Contains("2 endpoints", result);
        Assert.Contains("1 controllers", result);
    }

    // -------------------------------------------------------------------------
    // ArchitectureOverviewGenerator
    // -------------------------------------------------------------------------

    [Fact]
    public void ArchitectureOverview_WithObservations_ShowsThem()
    {
        var arch = new ArchitectureInfo(
            [new LayerInfo("Domain", ["MyApp.Domain"])],
            ["Clean Architecture detected.", "Uses Serilog."]);
        var analysis = new RepositoryAnalysisResult([], [], [], [], [], arch);
        var gen = new ArchitectureOverviewGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("## Observations", result);
        Assert.Contains("Clean Architecture detected.", result);
        Assert.Contains("Uses Serilog.", result);
    }

    [Fact]
    public void ArchitectureOverview_WithProjectReferences_ShowsThem()
    {
        var projects = new List<ProjectInfo>
        {
            new("API", "/src/API.csproj", "net10.0", "Web", ["Domain"], []),
            new("Domain", "/src/Domain.csproj", "net10.0", "Library", [], []),
        };
        var arch = new ArchitectureInfo([new LayerInfo("Presentation", ["API"]), new LayerInfo("Domain", ["Domain"])], []);
        var analysis = new RepositoryAnalysisResult(projects, [], [], [],
            [new DependencyLink("API", "Domain")],
            arch);
        var gen = new ArchitectureOverviewGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("**References:** Domain", result);
        // Dependencies appear in the mermaid block (layers required)
        Assert.Contains("API --> Domain", result);
    }

    [Fact]
    public void ArchitectureOverview_PackageList_ShowsDetails()
    {
        var projects = new List<ProjectInfo>
        {
            new("API", "/src/API.csproj", "net10.0", "Web", [],
                [new PackageInfo("Serilog", "3.0.0"), new PackageInfo("Npgsql", "9.0.0")]),
        };
        var analysis = new RepositoryAnalysisResult(projects, [], [], [], [], new ArchitectureInfo([], []));
        var gen = new ArchitectureOverviewGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("Serilog", result);
        Assert.Contains("Npgsql", result);
        Assert.Contains("<details>", result);
    }

    [Fact]
    public void ArchitectureOverview_NoLayers_SkipsMermaid()
    {
        var analysis = new RepositoryAnalysisResult([], [], [], [], [], new ArchitectureInfo([], []));
        var gen = new ArchitectureOverviewGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.DoesNotContain("```mermaid", result);
    }

    [Fact]
    public void ArchitectureOverview_MultipleLayers_AllAppearInDiagram()
    {
        var arch = new ArchitectureInfo(
        [
            new LayerInfo("Presentation", ["MyApp.API"]),
            new LayerInfo("Domain", ["MyApp.Domain"]),
            new LayerInfo("Infrastructure", ["MyApp.Infrastructure"]),
        ], []);
        var analysis = new RepositoryAnalysisResult([], [], [], [], [], arch);
        var gen = new ArchitectureOverviewGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("Presentation", result);
        Assert.Contains("Domain", result);
        Assert.Contains("Infrastructure", result);
    }

    // -------------------------------------------------------------------------
    // ReadmeGenerator
    // -------------------------------------------------------------------------

    [Fact]
    public void Readme_WithMoreThan20Entities_ShowsEllipsis()
    {
        var entities = Enumerable.Range(1, 25)
            .Select(i => MakeEntity($"Entity{i}"))
            .ToList();
        var analysis = new RepositoryAnalysisResult([], [], entities, [], [], new ArchitectureInfo([], []));
        var gen = new ReadmeGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("and 5 more", result);
    }

    [Fact]
    public void Readme_WithDependencies_ShowsMermaidGraph()
    {
        var analysis = new RepositoryAnalysisResult([], [], [], [],
            [new DependencyLink("API", "Domain"), new DependencyLink("API", "Infra")],
            new ArchitectureInfo([], []));
        var gen = new ReadmeGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("```mermaid", result);
        Assert.Contains("API --> Domain", result);
        Assert.Contains("API --> Infra", result);
    }

    [Fact]
    public void Readme_WithServices_ShowsServicesSection()
    {
        var services = new List<ServiceInfo> { MakeService(), MakeService("IOrderService", "OrderService") };
        var analysis = new RepositoryAnalysisResult([], [], [], services, [], new ArchitectureInfo([], []));
        var gen = new ReadmeGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("## Services", result);
        Assert.Contains("**2**", result); // "Total: **2** service interfaces discovered."
    }

    [Fact]
    public void Readme_WithArchitectureLayers_ShowsLayer()
    {
        var arch = new ArchitectureInfo(
            [new LayerInfo("Presentation", ["API"]), new LayerInfo("Domain", ["Domain"])],
            []);
        var analysis = new RepositoryAnalysisResult([], [], [], [], [], arch);
        var gen = new ReadmeGenerator();

        var result = gen.Generate(analysis, "Proj");

        Assert.Contains("## Architecture", result);
        Assert.Contains("**Presentation**", result);
        Assert.Contains("**Domain**", result);
    }

    [Fact]
    public void Readme_GeneratedByFooter_Present()
    {
        var gen = new ReadmeGenerator();
        var result = gen.Generate(EmptyAnalysis(), "Proj");

        Assert.Contains("Generated by RVM.DocForge", result);
    }

    // -------------------------------------------------------------------------
    // FullDocumentationGenerator
    // -------------------------------------------------------------------------

    [Fact]
    public void FullDoc_ContainsAnchorLinks()
    {
        var gen = new FullDocumentationGenerator();
        var result = gen.Generate(EmptyAnalysis(), "Proj");

        Assert.Contains("<a id=\"overview\"", result);
        Assert.Contains("<a id=\"architecture\"", result);
        Assert.Contains("<a id=\"api-reference\"", result);
        Assert.Contains("<a id=\"entity-schema\"", result);
        Assert.Contains("<a id=\"service-catalog\"", result);
        Assert.Contains("<a id=\"dependencies\"", result);
    }

    [Fact]
    public void FullDoc_TableOfContents_HasAllSections()
    {
        var gen = new FullDocumentationGenerator();
        var result = gen.Generate(EmptyAnalysis(), "Proj");

        Assert.Contains("[Overview]", result);
        Assert.Contains("[Architecture]", result);
        Assert.Contains("[API Reference]", result);
        Assert.Contains("[Entity Schema]", result);
        Assert.Contains("[Service Catalog]", result);
        Assert.Contains("[Dependencies]", result);
    }

    [Fact]
    public void FullDoc_WithData_CombinesAllSections()
    {
        var projects = new List<ProjectInfo>
        {
            new("API", "/src/API.csproj", "net10.0", "Web", [], [new PackageInfo("Serilog", "3.0.0")]),
        };
        var endpoints = new List<EndpointInfo> { MakeEndpoint() };
        var entities = new List<EntityInfo> { MakeEntity() };
        var services = new List<ServiceInfo> { MakeService() };
        var deps = new List<DependencyLink> { new("API", "Domain") };
        var arch = new ArchitectureInfo([new LayerInfo("API", ["API"])], ["Serilog detected."]);
        var analysis = new RepositoryAnalysisResult(projects, endpoints, entities, services, deps, arch);

        var gen = new FullDocumentationGenerator();
        var result = gen.Generate(analysis, "MyApp");

        Assert.Contains("# MyApp", result);
        Assert.Contains("# API Reference", result);
        Assert.Contains("# Entity Schema", result);
        Assert.Contains("# Architecture Overview", result);
        Assert.Contains("Serilog detected.", result);
        Assert.Contains("GET", result);
        Assert.Contains("User", result);
    }
}
