using System.Text;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public class DependencyGraphGenerator : IDocumentGenerator
{
    public string Generate(RepositoryAnalysisResult analysis, string projectName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# Dependency Graph — {projectName}");
        sb.AppendLine();

        // Project dependency graph
        if (analysis.Dependencies.Count > 0)
        {
            sb.AppendLine("## Project References");
            sb.AppendLine();
            sb.AppendLine("```mermaid");
            sb.AppendLine("graph LR");
            foreach (var dep in analysis.Dependencies)
            {
                var fromId = dep.From.Replace(".", "_");
                var toId = dep.To.Replace(".", "_");
                sb.AppendLine($"    {fromId}[\"{dep.From}\"] --> {toId}[\"{dep.To}\"]");
            }
            sb.AppendLine("```");
            sb.AppendLine();
        }

        // NuGet packages
        sb.AppendLine("## NuGet Packages");
        sb.AppendLine();

        var allPackages = analysis.Projects
            .SelectMany(p => p.Packages.Select(pkg => new { Project = p.Name, pkg.Name, pkg.Version }))
            .GroupBy(x => x.Name)
            .OrderBy(g => g.Key);

        sb.AppendLine("| Package | Version | Used By |");
        sb.AppendLine("|---------|---------|---------|");
        foreach (var group in allPackages)
        {
            var versions = string.Join(", ", group.Select(g => g.Version).Distinct());
            var usedBy = string.Join(", ", group.Select(g => g.Project).Distinct());
            sb.AppendLine($"| `{group.Key}` | {versions} | {usedBy} |");
        }
        sb.AppendLine();

        // Service dependencies (DI)
        if (analysis.Services.Count > 0)
        {
            sb.AppendLine("## Service Registrations");
            sb.AppendLine();
            sb.AppendLine("```mermaid");
            sb.AppendLine("graph LR");
            foreach (var svc in analysis.Services.Where(s => s.ImplementationName is not null))
            {
                var ifaceId = svc.InterfaceName.Replace(".", "_");
                var implId = svc.ImplementationName!.Replace(".", "_");
                var lifetime = svc.Lifetime ?? "?";
                sb.AppendLine($"    {ifaceId}[\"{svc.InterfaceName}\"] -->|{lifetime}| {implId}[\"{svc.ImplementationName}\"]");
            }
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
