using Microsoft.Extensions.Logging.Abstractions;
using RVM.DocForge.API.Services;

namespace RVM.DocForge.Test.Services;

/// <summary>
/// Testa o RepositoryAnalyzerService usando repositorios temporarios em disco.
/// </summary>
public class RepositoryAnalyzerServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly RepositoryAnalyzerService _service;

    public RepositoryAnalyzerServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"docforge-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _service = new RepositoryAnalyzerService(NullLogger<RepositoryAnalyzerService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private void WriteFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_tempDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
    }

    // -------------------------------------------------------------------------
    // AnalyzeAsync — repo vazio
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeAsync_EmptyRepo_ReturnsEmptyResult()
    {
        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Empty(result.Projects);
        Assert.Empty(result.Endpoints);
        Assert.Empty(result.Entities);
        Assert.Empty(result.Services);
        Assert.Empty(result.Dependencies);
    }

    // -------------------------------------------------------------------------
    // AnalyzeAsync — projetos csproj
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeAsync_SingleProject_ParsesTargetFramework()
    {
        WriteFile("src/MyApp.API/MyApp.API.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Single(result.Projects);
        Assert.Equal("net10.0", result.Projects[0].TargetFramework);
        Assert.Equal("MyApp.API", result.Projects[0].Name);
    }

    [Fact]
    public async Task AnalyzeAsync_ProjectWithPackages_ParsesPackages()
    {
        WriteFile("src/MyApp/MyApp.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Serilog" Version="3.0.0" />
                <PackageReference Include="Npgsql" Version="9.0.0" />
              </ItemGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        var project = Assert.Single(result.Projects);
        Assert.Equal(2, project.Packages.Count);
        Assert.Contains(project.Packages, p => p.Name == "Serilog");
        Assert.Contains(project.Packages, p => p.Name == "Npgsql");
    }

    [Fact]
    public async Task AnalyzeAsync_ProjectWithProjectReferences_CreatesDependencyLinks()
    {
        WriteFile("src/API/API.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="..\Domain\Domain.csproj" />
              </ItemGroup>
            </Project>
            """);
        WriteFile("src/Domain/Domain.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Contains(result.Dependencies, d => d.From == "API" && d.To == "Domain");
    }

    [Fact]
    public async Task AnalyzeAsync_WebSdk_InfersWebOutputType()
    {
        WriteFile("src/Web/Web.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Equal("Web", result.Projects[0].OutputType);
    }

    [Fact]
    public async Task AnalyzeAsync_ExplicitLibraryOutput_ParsesCorrectly()
    {
        WriteFile("src/Lib/Lib.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <OutputType>Library</OutputType>
              </PropertyGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Equal("Library", result.Projects[0].OutputType);
    }

    // -------------------------------------------------------------------------
    // AnalyzeAsync — extratores de codigo C#
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeAsync_WithController_ExtractsEndpoints()
    {
        WriteFile("src/API/API.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
        WriteFile("src/API/Controllers/UsersController.cs", """
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [Route("api/users")]
            public class UsersController : ControllerBase
            {
                [HttpGet]
                public IActionResult GetAll() => Ok();

                [HttpPost]
                public IActionResult Create([FromBody] object body) => Ok();
            }
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.True(result.Endpoints.Count >= 2);
        Assert.Contains(result.Endpoints, e => e.HttpMethod == "GET");
        Assert.Contains(result.Endpoints, e => e.HttpMethod == "POST");
    }

    [Fact]
    public async Task AnalyzeAsync_WithEntity_ExtractsEntityInfo()
    {
        WriteFile("src/Domain/Domain.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
        WriteFile("src/Domain/Entities/Order.cs", """
            namespace MyApp.Domain.Entities;

            /// <summary>Represents an order.</summary>
            public class Order
            {
                public Guid Id { get; set; }
                public decimal Total { get; set; }
            }
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Contains(result.Entities, e => e.Name == "Order" && e.Kind == "class");
    }

    [Fact]
    public async Task AnalyzeAsync_CancellationToken_ThrowsWhenCancelled()
    {
        WriteFile("src/API/API.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _service.AnalyzeAsync(_tempDir, cts.Token));
    }

    // -------------------------------------------------------------------------
    // InferArchitecture (exercised via AnalyzeAsync)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeAsync_CleanArchitecture_AddsLayers()
    {
        WriteFile("src/MyApp.API/MyApp.API.csproj", """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
        WriteFile("src/MyApp.Domain/MyApp.Domain.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
        WriteFile("src/MyApp.Infrastructure/MyApp.Infrastructure.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        var layerNames = result.Architecture.Layers.Select(l => l.Name).ToList();
        Assert.Contains("Presentation", layerNames);
        Assert.Contains("Domain", layerNames);
        Assert.Contains("Infrastructure", layerNames);
    }

    [Fact]
    public async Task AnalyzeAsync_WithTestProject_AddsTestsLayer()
    {
        WriteFile("test/MyApp.Tests/MyApp.Tests.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        var layerNames = result.Architecture.Layers.Select(l => l.Name).ToList();
        Assert.Contains("Tests", layerNames);
    }

    [Fact]
    public async Task AnalyzeAsync_WithEntityFramework_AddsObservation()
    {
        WriteFile("src/Infra/Infra.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
              </ItemGroup>
            </Project>
            """);

        var result = await _service.AnalyzeAsync(_tempDir);

        Assert.Contains(result.Architecture.Observations, o => o.Contains("Entity Framework Core"));
    }
}
