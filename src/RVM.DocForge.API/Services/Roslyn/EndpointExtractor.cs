using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RVM.DocForge.Domain.Models;

namespace RVM.DocForge.API.Services.Roslyn;

public class EndpointExtractor : CSharpSyntaxWalker
{
    private readonly SemanticModel? _semanticModel;
    private readonly List<EndpointInfo> _endpoints = [];
    private readonly string _projectName;

    private static readonly HashSet<string> HttpAttributes = new(StringComparer.OrdinalIgnoreCase)
    {
        "HttpGet", "HttpGetAttribute",
        "HttpPost", "HttpPostAttribute",
        "HttpPut", "HttpPutAttribute",
        "HttpDelete", "HttpDeleteAttribute",
        "HttpPatch", "HttpPatchAttribute"
    };

    public EndpointExtractor(string projectName, SemanticModel? semanticModel = null)
    {
        _projectName = projectName;
        _semanticModel = semanticModel;
    }

    public IReadOnlyList<EndpointInfo> Endpoints => _endpoints;

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (!IsController(node))
        {
            base.VisitClassDeclaration(node);
            return;
        }

        var controllerName = node.Identifier.Text;
        var classRoute = GetRouteTemplate(node.AttributeLists);

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            if (method.Modifiers.Any(SyntaxKind.PublicKeyword))
                TryExtractEndpoint(method, controllerName, classRoute);
        }

        base.VisitClassDeclaration(node);
    }

    private void TryExtractEndpoint(MethodDeclarationSyntax method, string controller, string? classRoute)
    {
        var (httpMethod, methodRoute) = GetHttpMethodAndRoute(method.AttributeLists);
        if (httpMethod is null) return;

        var route = CombineRoutes(classRoute, methodRoute);
        var parameters = ExtractParameters(method);
        var summary = ExtractXmlDocSummary(method);
        var responseType = method.ReturnType.ToString();

        string? requestType = null;
        var bodyParam = parameters.FirstOrDefault(p => p.Source == "Body");
        if (bodyParam != default) requestType = bodyParam.Type;

        _endpoints.Add(new EndpointInfo(
            controller, method.Identifier.Text, httpMethod, route,
            requestType, responseType, summary, parameters, _projectName));
    }

    private static bool IsController(ClassDeclarationSyntax node)
    {
        var hasAttribute = node.AttributeLists.SelectMany(a => a.Attributes)
            .Any(a => a.Name.ToString() is "ApiController" or "ApiControllerAttribute");

        var inherits = node.BaseList?.Types
            .Any(t => t.Type.ToString().Contains("Controller")) ?? false;

        return hasAttribute || inherits;
    }

    private static string? GetRouteTemplate(SyntaxList<AttributeListSyntax> attributeLists)
    {
        var routeAttr = attributeLists.SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString() is "Route" or "RouteAttribute");

        return routeAttr?.ArgumentList?.Arguments.FirstOrDefault()
            ?.Expression.ToString().Trim('"');
    }

    private static (string? Method, string? Route) GetHttpMethodAndRoute(SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attr in attributeLists.SelectMany(a => a.Attributes))
        {
            var name = attr.Name.ToString();
            if (!HttpAttributes.Contains(name)) continue;

            var method = name.Replace("Attribute", "").Replace("Http", "").ToUpper();
            var route = attr.ArgumentList?.Arguments.FirstOrDefault()
                ?.Expression.ToString().Trim('"');
            return (method, route);
        }

        return (null, null);
    }

    private static string CombineRoutes(string? classRoute, string? methodRoute)
    {
        if (classRoute is null && methodRoute is null) return "/";
        if (classRoute is null) return "/" + methodRoute;
        if (methodRoute is null) return "/" + classRoute;
        return "/" + classRoute.TrimEnd('/') + "/" + methodRoute.TrimStart('/');
    }

    private static List<ParameterInfo> ExtractParameters(MethodDeclarationSyntax method)
    {
        var parameters = new List<ParameterInfo>();
        foreach (var param in method.ParameterList.Parameters)
        {
            var source = "Query";
            foreach (var attr in param.AttributeLists.SelectMany(a => a.Attributes))
            {
                var attrName = attr.Name.ToString();
                if (attrName.Contains("FromBody")) source = "Body";
                else if (attrName.Contains("FromRoute")) source = "Route";
                else if (attrName.Contains("FromHeader")) source = "Header";
                else if (attrName.Contains("FromQuery")) source = "Query";
            }

            parameters.Add(new ParameterInfo(
                param.Identifier.Text,
                param.Type?.ToString() ?? "object",
                source));
        }
        return parameters;
    }

    private static string? ExtractXmlDocSummary(MethodDeclarationSyntax method)
    {
        var trivia = method.GetLeadingTrivia()
            .Select(t => t.GetStructure())
            .OfType<DocumentationCommentTriviaSyntax>()
            .FirstOrDefault();

        var summaryElement = trivia?.ChildNodes()
            .OfType<XmlElementSyntax>()
            .FirstOrDefault(e => e.StartTag.Name.ToString() == "summary");

        return summaryElement?.Content.ToString().Trim().Replace("///", "").Trim();
    }
}
