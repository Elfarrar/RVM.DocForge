using System.Text;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public class ServiceCatalogGenerator : IDocumentGenerator
{
    public string Generate(RepositoryAnalysisResult analysis, string projectName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# Service Catalog — {projectName}");
        sb.AppendLine();

        if (analysis.Services.Count == 0)
        {
            sb.AppendLine("*No service interfaces discovered.*");
            return sb.ToString();
        }

        sb.AppendLine($"> {analysis.Services.Count} service interfaces discovered");
        sb.AppendLine();

        // Summary table
        sb.AppendLine("## Overview");
        sb.AppendLine();
        sb.AppendLine("| Interface | Implementation | Lifetime | Methods |");
        sb.AppendLine("|-----------|---------------|----------|---------|");
        foreach (var svc in analysis.Services.OrderBy(s => s.InterfaceName))
        {
            var impl = svc.ImplementationName ?? "—";
            var lifetime = svc.Lifetime ?? "—";
            sb.AppendLine($"| `{svc.InterfaceName}` | `{impl}` | {lifetime} | {svc.MethodSignatures.Count} |");
        }
        sb.AppendLine();

        // Detailed view
        sb.AppendLine("## Details");
        sb.AppendLine();
        foreach (var svc in analysis.Services.OrderBy(s => s.InterfaceName))
        {
            sb.AppendLine($"### `{svc.InterfaceName}`");
            sb.AppendLine();

            if (svc.Summary is not null)
            {
                sb.AppendLine($"> {svc.Summary}");
                sb.AppendLine();
            }

            if (svc.ImplementationName is not null)
                sb.AppendLine($"**Implementation:** `{svc.ImplementationName}`");
            if (svc.Lifetime is not null)
                sb.AppendLine($"**Lifetime:** {svc.Lifetime}");
            if (svc.ProjectName is not null)
                sb.AppendLine($"**Project:** {svc.ProjectName}");

            sb.AppendLine();

            if (svc.MethodSignatures.Count > 0)
            {
                sb.AppendLine("**Methods:**");
                sb.AppendLine();
                sb.AppendLine("```csharp");
                foreach (var method in svc.MethodSignatures)
                {
                    sb.AppendLine(method);
                }
                sb.AppendLine("```");
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
