using System.Text;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public class FullDocumentationGenerator : IDocumentGenerator
{
    private readonly ReadmeGenerator _readme = new();
    private readonly ApiReferenceGenerator _apiRef = new();
    private readonly EntitySchemaGenerator _entitySchema = new();
    private readonly ArchitectureOverviewGenerator _architecture = new();
    private readonly DependencyGraphGenerator _dependencies = new();
    private readonly ServiceCatalogGenerator _serviceCatalog = new();

    public string Generate(RepositoryAnalysisResult analysis, string projectName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Table of Contents");
        sb.AppendLine();
        sb.AppendLine("1. [Overview](#overview)");
        sb.AppendLine("2. [Architecture](#architecture)");
        sb.AppendLine("3. [API Reference](#api-reference)");
        sb.AppendLine("4. [Entity Schema](#entity-schema)");
        sb.AppendLine("5. [Service Catalog](#service-catalog)");
        sb.AppendLine("6. [Dependencies](#dependencies)");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 1. Readme (overview)
        sb.AppendLine("<a id=\"overview\"></a>");
        sb.AppendLine();
        sb.AppendLine(_readme.Generate(analysis, projectName));
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 2. Architecture
        sb.AppendLine("<a id=\"architecture\"></a>");
        sb.AppendLine();
        sb.AppendLine(_architecture.Generate(analysis, projectName));
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 3. API Reference
        sb.AppendLine("<a id=\"api-reference\"></a>");
        sb.AppendLine();
        sb.AppendLine(_apiRef.Generate(analysis, projectName));
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 4. Entity Schema
        sb.AppendLine("<a id=\"entity-schema\"></a>");
        sb.AppendLine();
        sb.AppendLine(_entitySchema.Generate(analysis, projectName));
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 5. Service Catalog
        sb.AppendLine("<a id=\"service-catalog\"></a>");
        sb.AppendLine();
        sb.AppendLine(_serviceCatalog.Generate(analysis, projectName));
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // 6. Dependencies
        sb.AppendLine("<a id=\"dependencies\"></a>");
        sb.AppendLine();
        sb.AppendLine(_dependencies.Generate(analysis, projectName));

        return sb.ToString();
    }
}
