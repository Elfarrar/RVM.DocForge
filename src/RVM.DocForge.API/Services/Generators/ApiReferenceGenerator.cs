using System.Text;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public class ApiReferenceGenerator : IDocumentGenerator
{
    public string Generate(RepositoryAnalysisResult analysis, string projectName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"# API Reference — {projectName}");
        sb.AppendLine();

        if (analysis.Endpoints.Count == 0)
        {
            sb.AppendLine("*No API endpoints discovered.*");
            return sb.ToString();
        }

        sb.AppendLine($"> {analysis.Endpoints.Count} endpoints across {analysis.Endpoints.Select(e => e.Controller).Distinct().Count()} controllers");
        sb.AppendLine();

        foreach (var group in analysis.Endpoints.GroupBy(e => e.Controller))
        {
            sb.AppendLine($"## {group.Key}");
            sb.AppendLine();

            foreach (var ep in group)
            {
                sb.AppendLine($"### `{ep.HttpMethod} {ep.Route}`");
                sb.AppendLine();

                if (ep.Summary is not null)
                {
                    sb.AppendLine($"> {ep.Summary}");
                    sb.AppendLine();
                }

                sb.AppendLine($"**Action:** `{ep.Action}`");
                sb.AppendLine();

                if (ep.Parameters.Count > 0)
                {
                    sb.AppendLine("**Parameters:**");
                    sb.AppendLine();
                    sb.AppendLine("| Name | Type | Source |");
                    sb.AppendLine("|------|------|--------|");
                    foreach (var p in ep.Parameters)
                    {
                        sb.AppendLine($"| `{p.Name}` | `{p.Type}` | {p.Source} |");
                    }
                    sb.AppendLine();
                }

                if (ep.RequestType is not null)
                {
                    sb.AppendLine($"**Request Body:** `{ep.RequestType}`");
                    sb.AppendLine();
                }

                if (ep.ResponseType is not null)
                {
                    sb.AppendLine($"**Response:** `{ep.ResponseType}`");
                    sb.AppendLine();
                }

                sb.AppendLine("---");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}
