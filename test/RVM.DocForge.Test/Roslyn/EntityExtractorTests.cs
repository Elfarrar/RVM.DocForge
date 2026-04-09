using Microsoft.CodeAnalysis.CSharp;
using RVM.DocForge.API.Services.Roslyn;

namespace RVM.DocForge.Test.Roslyn;

public class EntityExtractorTests
{
    [Fact]
    public void Should_Extract_Class_With_Properties()
    {
        var code = """
            namespace MyApp.Domain;

            public class User
            {
                public Guid Id { get; set; }
                public string Name { get; set; } = string.Empty;
                public string? Email { get; set; }
            }
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Entities);
        var entity = extractor.Entities[0];
        Assert.Equal("User", entity.Name);
        Assert.Equal("class", entity.Kind);
        Assert.Equal("MyApp.Domain", entity.Namespace);
        Assert.Equal(3, entity.Properties.Count);
    }

    [Fact]
    public void Should_Extract_Nullable_Properties()
    {
        var code = """
            public class Order
            {
                public int Id { get; set; }
                public string? Notes { get; set; }
            }
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Entities);
        var notes = extractor.Entities[0].Properties.First(p => p.Name == "Notes");
        Assert.True(notes.IsNullable);
    }

    [Fact]
    public void Should_Extract_Record()
    {
        var code = """
            namespace MyApp.Models;

            public record CreateUserRequest(string Name, string Email);
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Entities);
        var entity = extractor.Entities[0];
        Assert.Equal("CreateUserRequest", entity.Name);
        Assert.Equal("record", entity.Kind);
        Assert.Equal(2, entity.Properties.Count);
    }

    [Fact]
    public void Should_Extract_Enum()
    {
        var code = """
            namespace MyApp.Enums;

            public enum Status
            {
                Active,
                Inactive,
                Suspended
            }
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Entities);
        var entity = extractor.Entities[0];
        Assert.Equal("Status", entity.Name);
        Assert.Equal("enum", entity.Kind);
        Assert.Equal(3, entity.Properties.Count);
    }

    [Fact]
    public void Should_Extract_BaseType_And_Interfaces()
    {
        var code = """
            public interface IAuditable { }

            public class BaseEntity
            {
                public Guid Id { get; set; }
            }

            public class User : BaseEntity, IAuditable
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        var user = extractor.Entities.FirstOrDefault(e => e.Name == "User");
        Assert.NotNull(user);
        Assert.Equal("BaseEntity", user.BaseType);
        Assert.Contains("IAuditable", user.Interfaces);
    }

    [Fact]
    public void Should_Ignore_Abstract_Classes()
    {
        var code = """
            public abstract class BaseEntity
            {
                public Guid Id { get; set; }
            }
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Empty(extractor.Entities);
    }

    [Fact]
    public void Should_Ignore_Static_Classes()
    {
        var code = """
            public static class Extensions
            {
                public static string SomeProperty => "value";
            }
            """;

        var extractor = new EntityExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Empty(extractor.Entities);
    }
}
