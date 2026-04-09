using Microsoft.CodeAnalysis.CSharp;
using RVM.DocForge.API.Services.Roslyn;

namespace RVM.DocForge.Test.Roslyn;

public class ServiceExtractorTests
{
    [Fact]
    public void Should_Extract_Interface_With_Methods()
    {
        var code = """
            public interface IUserService
            {
                Task<User> GetByIdAsync(Guid id);
                Task CreateAsync(User user);
            }
            """;

        var extractor = new ServiceExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Services);
        var svc = extractor.Services[0];
        Assert.Equal("IUserService", svc.InterfaceName);
        Assert.Equal(2, svc.MethodSignatures.Count);
    }

    [Fact]
    public void Should_Match_Implementation_To_Interface()
    {
        // Class must appear before interface — SyntaxWalker visits in document order
        var code = """
            public class UserService : IUserService
            {
                public Task DoWork() => Task.CompletedTask;
            }

            public interface IUserService
            {
                Task DoWork();
            }
            """;

        var extractor = new ServiceExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        Assert.Single(extractor.Services);
        Assert.Equal("UserService", extractor.Services[0].ImplementationName);
    }

    [Fact]
    public void Should_Extract_DI_Lifetime_AddScoped()
    {
        var code = """
            public static class DI
            {
                public static void Configure(IServiceCollection services)
                {
                    services.AddScoped<IUserService, UserService>();
                }
            }

            public interface IUserService
            {
                Task DoWork();
            }
            """;

        var extractor = new ServiceExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        var svc = extractor.Services.FirstOrDefault(s => s.InterfaceName == "IUserService");
        Assert.NotNull(svc);
        Assert.Equal("Scoped", svc.Lifetime);
        Assert.Equal("UserService", svc.ImplementationName);
    }

    [Fact]
    public void Should_Extract_Singleton_Lifetime()
    {
        var code = """
            public static class DI
            {
                public static void Configure(IServiceCollection services)
                {
                    services.AddSingleton<ICacheService, CacheService>();
                }
            }

            public interface ICacheService
            {
                string Get(string key);
            }
            """;

        var extractor = new ServiceExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        var svc = extractor.Services.FirstOrDefault(s => s.InterfaceName == "ICacheService");
        Assert.NotNull(svc);
        Assert.Equal("Singleton", svc.Lifetime);
    }

    [Fact]
    public void Should_Extract_Transient_Lifetime()
    {
        var code = """
            public static class DI
            {
                public static void Configure(IServiceCollection services)
                {
                    services.AddTransient<IEmailSender, EmailSender>();
                }
            }

            public interface IEmailSender
            {
                Task Send(string to, string body);
            }
            """;

        var extractor = new ServiceExtractor("TestProject");
        var tree = CSharpSyntaxTree.ParseText(code);
        extractor.Visit(tree.GetRoot());

        var svc = extractor.Services.FirstOrDefault(s => s.InterfaceName == "IEmailSender");
        Assert.NotNull(svc);
        Assert.Equal("Transient", svc.Lifetime);
    }
}
