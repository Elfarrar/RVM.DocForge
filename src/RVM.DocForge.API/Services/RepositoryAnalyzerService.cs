using System.Text.Json;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.CSharp;
using RVM.DocForge.API.Services.Roslyn;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services;

public class RepositoryAnalyzerService(ILogger<RepositoryAnalyzerService> logger)
{
    public async Task<RepositoryAnalysisResult> AnalyzeAsync(string repositoryPath, CancellationToken ct = default)
    {
        logger.LogInformation("Starting analysis of repository at {Path}", repositoryPath);

        var projects = await DiscoverProjectsAsync(repositoryPath, ct);
        var endpoints = new List<EndpointInfo>();
        var entities = new List<EntityInfo>();
        var services = new List<ServiceInfo>();
        var dependencies = new List<DependencyLink>();

        foreach (var project in projects)
        {
            ct.ThrowIfCancellationRequested();

            var projectDir = Path.GetDirectoryName(project.Path)!;
            var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories);

            logger.LogInformation("Analyzing project {Name} — {FileCount} .cs files", project.Name, csFiles.Length);

            foreach (var file in csFiles)
            {
                ct.ThrowIfCancellationRequested();

                var code = await File.ReadAllTextAsync(file, ct);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = await tree.GetRootAsync(ct);

                // Endpoints
                var epExtractor = new EndpointExtractor(project.Name);
                epExtractor.Visit(root);
                endpoints.AddRange(epExtractor.Endpoints);

                // Entities
                var entityExtractor = new EntityExtractor(project.Name);
                entityExtractor.Visit(root);
                entities.AddRange(entityExtractor.Entities);

                // Services
                var svcExtractor = new ServiceExtractor(project.Name);
                svcExtractor.Visit(root);
                services.AddRange(svcExtractor.Services);
            }

            // Project references as dependencies
            foreach (var refName in project.ProjectReferences)
            {
                dependencies.Add(new DependencyLink(project.Name, refName));
            }
        }

        var architecture = InferArchitecture(projects);

        logger.LogInformation(
            "Analysis complete: {Projects} projects, {Endpoints} endpoints, {Entities} entities, {Services} services",
            projects.Count, endpoints.Count, entities.Count, services.Count);

        return new RepositoryAnalysisResult(projects, endpoints, entities, services, dependencies, architecture);
    }

    private static async Task<List<ProjectInfo>> DiscoverProjectsAsync(string repoPath, CancellationToken ct)
    {
        var csprojFiles = Directory.GetFiles(repoPath, "*.csproj", SearchOption.AllDirectories);
        var projects = new List<ProjectInfo>();

        foreach (var csproj in csprojFiles)
        {
            ct.ThrowIfCancellationRequested();

            var xml = await File.ReadAllTextAsync(csproj, ct);
            var doc = XDocument.Parse(xml);
            var name = Path.GetFileNameWithoutExtension(csproj);

            var targetFramework = doc.Descendants("TargetFramework").FirstOrDefault()?.Value ?? "unknown";
            var outputType = doc.Descendants("OutputType").FirstOrDefault()?.Value ?? InferOutputType(doc);

            var projectRefs = doc.Descendants("ProjectReference")
                .Select(pr => Path.GetFileNameWithoutExtension(pr.Attribute("Include")?.Value ?? ""))
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();

            var packages = doc.Descendants("PackageReference")
                .Select(pr => new PackageInfo(
                    pr.Attribute("Include")?.Value ?? "",
                    pr.Attribute("Version")?.Value ?? ""))
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .ToList();

            projects.Add(new ProjectInfo(name, csproj, targetFramework, outputType, projectRefs, packages));
        }

        return projects;
    }

    private static string InferOutputType(XDocument doc)
    {
        var sdk = doc.Root?.Attribute("Sdk")?.Value ?? "";
        if (sdk.Contains("Web")) return "Web";
        if (sdk.Contains("Worker")) return "Worker";
        return "Library";
    }

    private static ArchitectureInfo InferArchitecture(List<ProjectInfo> projects)
    {
        var layers = new List<LayerInfo>();
        var observations = new List<string>();

        var apiProjects = projects.Where(p => p.OutputType == "Web" || p.Name.Contains("API", StringComparison.OrdinalIgnoreCase)).ToList();
        var domainProjects = projects.Where(p => p.Name.Contains("Domain", StringComparison.OrdinalIgnoreCase)).ToList();
        var infraProjects = projects.Where(p => p.Name.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase)).ToList();
        var testProjects = projects.Where(p => p.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) || p.Name.Contains("Tests", StringComparison.OrdinalIgnoreCase)).ToList();

        var categorized = new HashSet<string>(
            apiProjects.Concat(domainProjects).Concat(infraProjects).Concat(testProjects).Select(p => p.Name));
        var otherProjects = projects.Where(p => !categorized.Contains(p.Name)).ToList();

        if (apiProjects.Count > 0) layers.Add(new LayerInfo("Presentation", apiProjects.Select(p => p.Name).ToList()));
        if (domainProjects.Count > 0) layers.Add(new LayerInfo("Domain", domainProjects.Select(p => p.Name).ToList()));
        if (infraProjects.Count > 0) layers.Add(new LayerInfo("Infrastructure", infraProjects.Select(p => p.Name).ToList()));
        if (testProjects.Count > 0) layers.Add(new LayerInfo("Tests", testProjects.Select(p => p.Name).ToList()));
        if (otherProjects.Count > 0) layers.Add(new LayerInfo("Other", otherProjects.Select(p => p.Name).ToList()));

        if (layers.Count >= 3)
            observations.Add("Project follows Clean Architecture pattern (Domain, Infrastructure, Presentation layers detected).");
        if (testProjects.Count > 0)
            observations.Add($"Test coverage: {testProjects.Count} test project(s) detected.");
        if (projects.Any(p => p.Packages.Any(pkg => pkg.Name.Contains("EntityFramework"))))
            observations.Add("Uses Entity Framework Core for data access.");
        if (projects.Any(p => p.Packages.Any(pkg => pkg.Name.Contains("Serilog"))))
            observations.Add("Uses Serilog for structured logging.");

        return new ArchitectureInfo(layers, observations);
    }
}
