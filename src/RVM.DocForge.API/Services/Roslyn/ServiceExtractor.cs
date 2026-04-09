using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Roslyn;

public class ServiceExtractor : CSharpSyntaxWalker
{
    private readonly List<ServiceInfo> _services = [];
    private readonly Dictionary<string, string> _implementations = new();
    private readonly Dictionary<string, string> _lifetimes = new();
    private readonly string _projectName;

    public ServiceExtractor(string projectName)
    {
        _projectName = projectName;
    }

    public IReadOnlyList<ServiceInfo> Services => _services;

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        var methods = node.Members.OfType<MethodDeclarationSyntax>()
            .Select(m => $"{m.ReturnType} {m.Identifier.Text}({string.Join(", ", m.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}"))})")
            .ToList();

        var interfaceName = node.Identifier.Text;

        _services.Add(new ServiceInfo(
            interfaceName,
            _implementations.GetValueOrDefault(interfaceName),
            ExtractSummary(node),
            methods,
            _lifetimes.GetValueOrDefault(interfaceName),
            _projectName));

        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (node.BaseList is not null)
        {
            foreach (var baseType in node.BaseList.Types)
            {
                var typeName = baseType.Type.ToString();
                if (typeName.StartsWith("I") && char.IsUpper(typeName[1]))
                    _implementations[typeName] = node.Identifier.Text;
            }
        }

        base.VisitClassDeclaration(node);
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var expr = node.Expression.ToString();
        if (expr.Contains("AddScoped") || expr.Contains("AddTransient") || expr.Contains("AddSingleton"))
        {
            var lifetime = expr.Contains("AddScoped") ? "Scoped"
                : expr.Contains("AddTransient") ? "Transient"
                : "Singleton";

            if (node.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax generic &&
                generic.TypeArgumentList.Arguments.Count >= 1)
            {
                var interfaceType = generic.TypeArgumentList.Arguments[0].ToString();
                _lifetimes[interfaceType] = lifetime;

                if (generic.TypeArgumentList.Arguments.Count >= 2)
                    _implementations[interfaceType] = generic.TypeArgumentList.Arguments[1].ToString();
            }
        }

        base.VisitInvocationExpression(node);
    }

    private static string? ExtractSummary(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        var summary = trivia?.ChildNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(e => e.StartTag.Name.ToString() == "summary");

        return summary?.Content.ToString().Trim().Replace("///", "").Trim();
    }
}
