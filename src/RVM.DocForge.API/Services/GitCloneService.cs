using LibGit2Sharp;

namespace RVM.DocForge.API.Services;

public class GitCloneService(ILogger<GitCloneService> logger)
{
    private static readonly string BaseCloneDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RVM.DocForge", "repos");

    /// <summary>
    /// Clones a public Git repository to a persistent local directory.
    /// Returns the clone path.
    /// </summary>
    public string Clone(string repositoryUrl)
    {
        var repoName = ExtractRepoName(repositoryUrl);
        var clonePath = Path.Combine(BaseCloneDir, $"{repoName}-{Guid.NewGuid().ToString()[..8]}");

        Directory.CreateDirectory(clonePath);

        logger.LogInformation("Cloning {Url} to {Path}", repositoryUrl, clonePath);

        Repository.Clone(repositoryUrl, clonePath, new CloneOptions
        {
            IsBare = false,
            RecurseSubmodules = false
        });

        logger.LogInformation("Clone complete: {Path}", clonePath);
        return clonePath;
    }

    /// <summary>
    /// Pulls latest changes for an existing clone.
    /// </summary>
    public void Pull(string clonePath)
    {
        if (!Directory.Exists(Path.Combine(clonePath, ".git")))
        {
            logger.LogWarning("Not a git repository: {Path}", clonePath);
            return;
        }

        using var repo = new Repository(clonePath);
        var remote = repo.Network.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification).ToList();

        Commands.Fetch(repo, remote.Name, refSpecs, null, "pull");

        var originMain = repo.Branches.FirstOrDefault(b => b.IsRemote && b.FriendlyName.Contains("origin/"))
            ?? throw new InvalidOperationException("No remote tracking branch found.");

        repo.Reset(ResetMode.Hard, originMain.Tip);
        logger.LogInformation("Pulled latest for {Path}", clonePath);
    }

    private static string ExtractRepoName(string url)
    {
        var uri = url.TrimEnd('/');
        if (uri.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            uri = uri[..^4];
        var lastSlash = uri.LastIndexOf('/');
        return lastSlash >= 0 ? uri[(lastSlash + 1)..] : "repo";
    }
}
