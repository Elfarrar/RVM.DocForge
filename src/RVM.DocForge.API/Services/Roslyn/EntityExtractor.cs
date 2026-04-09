using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Roslyn;

public class EntityExtractor : CSharpSyntaxWalker
{
    private readonly List<EntityInfo> _entities = [];
    private readonly string _projectName;

    public EntityExtractor(string projectName)
    {
        _projectName = projectName;
    }

    public IReadOnlyList<EntityInfo> Entities => _entities;

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (IsEntity(node))
            ExtractType(node, "class");
        base.VisitClassDeclaration(node);
    }

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        ExtractType(node, node.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) ? "record struct" : "record");
        base.VisitRecordDeclaration(node);
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        var ns = GetNamespace(node);
        var members = node.Members.Select(m => new PropertyDetail(m.Identifier.Text, "enum", false, null, m.EqualsValue?.Value.ToString())).ToList();

        _entities.Add(new EntityInfo(
            node.Identifier.Text, ns, "enum", ExtractSummary(node),
            members, null, [], _projectName));

        base.VisitEnumDeclaration(node);
    }

    private void ExtractType(TypeDeclarationSyntax node, string kind)
    {
        var ns = GetNamespace(node);
        var properties = node.Members.OfType<PropertyDeclarationSyntax>()
            .Select(p => new PropertyDetail(
                p.Identifier.Text,
                p.Type.ToString(),
                p.Type is NullableTypeSyntax,
                null,
                p.Initializer?.Value.ToString()))
            .ToList();

        // Record positional parameters
        if (node is RecordDeclarationSyntax record && record.ParameterList is not null)
        {
            foreach (var param in record.ParameterList.Parameters)
            {
                properties.Add(new PropertyDetail(
                    param.Identifier.Text,
                    param.Type?.ToString() ?? "object",
                    param.Type is NullableTypeSyntax,
                    null, null));
            }
        }

        var baseType = node.BaseList?.Types.FirstOrDefault()?.Type.ToString();
        var interfaces = node.BaseList?.Types.Skip(baseType?.StartsWith("I") == true ? 0 : 1)
            .Select(t => t.Type.ToString()).ToList() ?? [];

        if (baseType?.StartsWith("I") == true)
        {
            interfaces.Insert(0, baseType);
            baseType = null;
        }

        _entities.Add(new EntityInfo(
            node.Identifier.Text, ns, kind, ExtractSummary(node),
            properties, baseType, interfaces, _projectName));
    }

    private static bool IsEntity(ClassDeclarationSyntax node)
    {
        if (node.Modifiers.Any(SyntaxKind.AbstractKeyword)) return false;
        if (node.Modifiers.Any(SyntaxKind.StaticKeyword)) return false;

        var hasProperties = node.Members.OfType<PropertyDeclarationSyntax>().Any();
        return hasProperties;
    }

    private static string GetNamespace(SyntaxNode node)
    {
        var ns = node.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
        if (ns is not null) return ns.Name.ToString();

        var blockNs = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return blockNs?.Name.ToString() ?? "";
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
