using Microsoft.Extensions.Logging.Abstractions;
using RVM.DocForge.API.Services;

namespace RVM.DocForge.Test.Services;

/// <summary>
/// Testa a logica estatica do GitCloneService via reflexao (ExtractRepoName)
/// e verifica comportamento do Pull em diretorios invalidos sem rede.
/// </summary>
public class GitCloneServiceTests
{
    private readonly GitCloneService _service =
        new(NullLogger<GitCloneService>.Instance);

    // Acessamos o metodo privado ExtractRepoName via reflexao
    private static string ExtractRepoName(string url)
    {
        var method = typeof(GitCloneService).GetMethod(
            "ExtractRepoName",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        return (string)method.Invoke(null, [url])!;
    }

    // -------------------------------------------------------------------------
    // ExtractRepoName
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("https://github.com/org/my-repo.git", "my-repo")]
    [InlineData("https://github.com/org/my-repo", "my-repo")]
    [InlineData("https://github.com/org/my-repo/", "my-repo")]
    [InlineData("https://github.com/org/My.Repo.git", "My.Repo")]
    public void ExtractRepoName_VariousUrls_ReturnsCorrectName(string url, string expected)
    {
        var name = ExtractRepoName(url);
        Assert.Equal(expected, name);
    }

    [Fact]
    public void ExtractRepoName_UrlWithoutSlash_ReturnsUrl()
    {
        // fallback — sem barra, retorna "repo"
        var name = ExtractRepoName("noslash");
        Assert.Equal("repo", name);
    }

    [Fact]
    public void ExtractRepoName_EndsWithGitExtension_Strips()
    {
        var name = ExtractRepoName("https://example.com/repos/cool-project.GIT");
        Assert.Equal("cool-project", name);
    }

    // -------------------------------------------------------------------------
    // Pull — caminho sem .git (sem rede)
    // -------------------------------------------------------------------------

    [Fact]
    public void Pull_NonGitDirectory_DoesNotThrow()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), $"nogit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tmpDir);

        try
        {
            // Nao deve lancar excecao — apenas loga warning
            var ex = Record.Exception(() => _service.Pull(tmpDir));
            Assert.Null(ex);
        }
        finally
        {
            Directory.Delete(tmpDir, recursive: true);
        }
    }

    [Fact]
    public void Pull_NonexistentDirectory_DoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), "totally-nonexistent-1234567890");

        // Pull checa Directory.Exists(path + "/.git") — nao existe, retorna silenciosamente
        var ex = Record.Exception(() => _service.Pull(fakePath));
        Assert.Null(ex);
    }
}
