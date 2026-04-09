using System.Text;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public class ArchitectureOverviewGenerator : IDocumentGenerator
{
    public string Generate(RepositoryAnalysisResult analysis, string projectName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# Architecture Overview — {projectName}");
        sb.AppendLine();

        // Layer diagram
        if (analysis.Architecture.Layers.Count > 0)
        {
            sb.AppendLine("## Layers");
            sb.AppendLine();
            sb.AppendLine("```mermaid");
            sb.AppendLine("graph TD");

            for (var i = 0; i < analysis.Architecture.Layers.Count; i++)
            {
                var layer = analysis.Architecture.Layers[i];
                var layerId = layer.Name.Replace(" ", "_").Replace(".", "_");
                sb.AppendLine($"    subgraph {layerId}[\"{layer.Name}\"]");
                foreach (var proj in layer.Projects)
                {
                    var projId = proj.Replace(".", "_");
                    sb.AppendLine($"        {projId}[\"{proj}\"]");
                }
                sb.AppendLine("    end");
            }

            // Layer dependencies
            foreach (var dep in analysis.Dependencies)
            {
                var fromId = dep.From.Replace(".", "_");
                var toId = dep.To.Replace(".", "_");
                sb.AppendLine($"    {fromId} --> {toId}");
            }

            sb.AppendLine("```");
            sb.AppendLine();
        }

        // Project details
        sb.AppendLine("## Projects");
        sb.AppendLine();
        foreach (var project in analysis.Projects)
        {
            sb.AppendLine($"### {project.Name}");
            sb.AppendLine();
            sb.AppendLine($"- **Framework:** {project.TargetFramework}");
            sb.AppendLine($"- **Output:** {project.OutputType}");

            if (project.ProjectReferences.Count > 0)
            {
                sb.AppendLine($"- **References:** {string.Join(", ", project.ProjectReferences)}");
            }

            if (project.Packages.Count > 0)
            {
                sb.AppendLine($"- **Packages:** {project.Packages.Count}");
                sb.AppendLine();
                sb.AppendLine("  <details><summary>Package list</summary>");
                sb.AppendLine();
                foreach (var pkg in project.Packages.OrderBy(p => p.Name))
                {
                    sb.AppendLine($"  - `{pkg.Name}` v{pkg.Version}");
                }
                sb.AppendLine();
                sb.AppendLine("  </details>");
            }

            sb.AppendLine();
        }

        // Observations
        if (analysis.Architecture.Observations.Count > 0)
        {
            sb.AppendLine("## Observations");
            sb.AppendLine();
            foreach (var obs in analysis.Architecture.Observations)
            {
                sb.AppendLine($"- {obs}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
