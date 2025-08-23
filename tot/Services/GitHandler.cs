using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.Git;
using tot_lib.Git.Models;
using Tot;
using Worktree = tot_lib.Git.Worktree;

namespace tot.Services;

public class GitHandler : ITotService
{
    private readonly KitchenFiles _files;

    public GitHandler(Config config, KitchenFiles files)
    {
        _files = files;
        GitUtils.GitBinary = config.GitBinary;
    }

    public async Task CommitFile(DirectoryInfo directory, FileInfo file, string message)
    {
        var repo = directory.FullName;
        if (!await IsRepositoryValid(repo))
            throw new Exception("Invalid repository");
        if (await HasAnyStagedChanges(repo))
            throw new Exception("Repository is dirty");
        
        var localFile = file.FullName.RemoveRootFolder(directory.FullName).PosixFullName();
        if (!await StageFile(repo, localFile))
            throw new Exception($"Could not stage {localFile}");
        await Commit(repo, message, false);
    }

    public async Task<bool> IsGitRepoInvalidOrDirty(DirectoryInfo directory)
    {
        if (!await IsRepositoryValid(directory.FullName)) return false;
        return await IsRepositoryDirty(directory.FullName);
    }

    public async Task<bool> HasDedicatedModsSharedBranch()
    {
        return await HasBranch(_files.ModsShared.FullName, _files.ModName);
    }

    public async Task<string> CheckoutModsSharedBranch()
    {
        var repo = _files.ModsShared.FullName;
        if (!await IsRepositoryValid(repo))
            throw new Exception($"Invalid repository {repo}");

        var branches = await GetReposBranches(repo);
        var branch = branches.FirstOrDefault(b => b.FriendlyName == _files.ModName);
        branch ??= branches.FirstOrDefault(b => b.FriendlyName == "master");
        
        if (branch == null)
            throw new Exception("Invalid ModsShared repository branches");

        var worktree = await GetCurrentWorktree();
        if (worktree.Name == branch.Name)
            return branch.FriendlyName;

        if (await IsGitRepoInvalidOrDirty(_files.ModsShared))
            throw new Exception("ModsShared Repository is dirty");

        if(!await Checkout(repo, branch.Name))
            throw new Exception($"Could not checkout {branch.FriendlyName}");
        return branch.FriendlyName;
    }

    public async Task<bool> IsModsSharedBranchValid()
    {
        if (!await IsRepositoryValid(_files.ModsShared.FullName))
            return false;

        var repo = await GetCurrentWorktree();
        return repo.Name == _files.ModName ||
               (!await HasDedicatedModsSharedBranch() && repo.Name == "master");
    }

    public async Task<tot_lib.Git.Models.Worktree> GetCurrentWorktree()
    {
        var query = new Worktree(_files.ModsShared.FullName);
        var results = await Task.Run(query.List);
        if(results.Count == 0)
            throw new Exception("No worktree found");
        if(results.Count > 1)
            throw new Exception("Multiple worktrees is unsupported");
        return results.First();
    }

    public async Task<List<Branch>> GetReposBranches(string repo)
    {
        var branchQuery = new QueryBranches(repo);
        return await Task.Run(branchQuery.Result);
    }

    public async Task<bool> HasBranch(string repo, string branch)
    {
        return (await GetReposBranches(repo)).Any(b => b.FriendlyName == branch);
    }

    public async Task<bool> IsModsSharedRepositoryValid()
    {
        return await IsRepositoryValid(_files.ModsShared.FullName);
    }

    public async Task<List<Change>> ListChanges(string repo, bool includeUntracked = true)
    {
        var query = new QueryLocalChanges(repo, includeUntracked);
        return await Task.Run(query.Result);
    }

    public async Task<bool> StageFile(string repo, string localFile)
    {
        var change = await GetFileChanges(repo, localFile);
        if (change == null) return false;
        if (change.WorkTree == ChangeState.None) return true;
        await GitUtils.StageChanges(repo, [change]);
        change = await GetFileChanges(repo, localFile);
        if (change == null) return false;
        if (change.WorkTree != ChangeState.None) return false;
        return true;
    }

    public async Task<Change?> GetFileChanges(string repo, string localFile)
    {
        var changes = await ListChanges(repo);
        return changes.FirstOrDefault(x => x.Path == localFile);
    }

    public async Task<bool> IsRepositoryDirty(string repo)
    {
        return (await ListChanges(repo)).Count != 0;
    }

    public async Task<bool> HasAnyStagedChanges(string repo)
    {
        var changes = await ListChanges(repo);
        return changes.Any(x => x.Index != ChangeState.None);
    }

    public async Task Commit(string repo, string message, bool amend)
    {
        var commit = new Commit(repo, message, amend, false);
        await Task.Run(commit.Run);
    }

    public async Task<bool> Checkout(string repo, string branch)
    {
        var checkout = new Checkout(repo);
        return await Task.Run(() => checkout.Branch(branch));
    } 

    public async Task<bool> IsRepositoryValid(string repo)
    {
        var query = new QueryGitDir(repo);
        var result = await Task.Run(query.ReadToEnd);
        return result.IsSuccess;
    }
}