using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Generators;

public interface IDocumentGenerator
{
    string Generate(RepositoryAnalysisResult analysis, string projectName);
}
