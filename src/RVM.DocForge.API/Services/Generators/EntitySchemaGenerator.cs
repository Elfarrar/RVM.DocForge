using System.Text;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public class EntitySchemaGenerator : IDocumentGenerator
{
    public string Generate(RepositoryAnalysisResult analysis, string projectName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# Entity Schema — {projectName}");
        sb.AppendLine();

        if (analysis.Entities.Count == 0)
        {
            sb.AppendLine("*No entities discovered.*");
            return sb.ToString();
        }

        sb.AppendLine($"> {analysis.Entities.Count} types discovered");
        sb.AppendLine();

        // Group by namespace
        foreach (var nsGroup in analysis.Entities.GroupBy(e => e.Namespace).OrderBy(g => g.Key))
        {
            if (!string.IsNullOrEmpty(nsGroup.Key))
            {
                sb.AppendLine($"## Namespace: `{nsGroup.Key}`");
                sb.AppendLine();
            }

            foreach (var entity in nsGroup.OrderBy(e => e.Name))
            {
                var kindBadge = entity.Kind switch
                {
                    "enum" => "enum",
                    "record" => "record",
                    "record struct" => "record struct",
                    _ => "class"
                };

                sb.AppendLine($"### `{entity.Name}` <small>({kindBadge})</small>");
                sb.AppendLine();

                if (entity.Summary is not null)
                {
                    sb.AppendLine($"> {entity.Summary}");
                    sb.AppendLine();
                }

                if (entity.BaseType is not null)
                    sb.AppendLine($"**Inherits:** `{entity.BaseType}`");

                if (entity.Interfaces.Count > 0)
                    sb.AppendLine($"**Implements:** {string.Join(", ", entity.Interfaces.Select(i => $"`{i}`"))}");

                if (entity.BaseType is not null || entity.Interfaces.Count > 0)
                    sb.AppendLine();

                if (entity.Properties.Count > 0)
                {
                    sb.AppendLine("| Property | Type | Nullable | Default |");
                    sb.AppendLine("|----------|------|----------|---------|");
                    foreach (var prop in entity.Properties)
                    {
                        var nullable = prop.IsNullable ? "Yes" : "No";
                        var def = prop.DefaultValue ?? "—";
                        sb.AppendLine($"| `{prop.Name}` | `{prop.Type}` | {nullable} | {def} |");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
