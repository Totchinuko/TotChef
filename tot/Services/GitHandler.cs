using tot_lib;
using tot_lib.Git;
using tot_lib.Git.Models;
using Worktree = tot_lib.Git.Worktree;

namespace tot.Services;

public class GitHandler(KitchenFiles files) : ITotService
{
    public async Task CommitFile(DirectoryInfo directory, FileInfo file, string message)
    {
        var repo = directory.FullName;
        if (!await IsRepositoryValid(repo))
            throw new CommandException(CommandCode.RepositoryInvalid, "Invalid repository");
        if (await HasAnyStagedChanges(repo))
            throw new CommandException(CommandCode.RepositoryIsDirty, "Repository is dirty");
        
        var localFile = file.FullName.RemoveRootFolder(directory.FullName);
        if (!await StageFile(repo, localFile))
            throw new CommandException($"Could not stage {localFile}");
        await Commit(repo, message, false);
    }

    public async Task<bool> IsGitRepoInvalidOrDirty(DirectoryInfo directory)
    {
        if (!await IsRepositoryValid(directory.FullName)) return false;
        return await IsRepositoryDirty(directory.FullName);
    }

    public async Task<bool> HasDedicatedModsSharedBranch()
    {
        return await HasBranch(files.ModsShared.FullName, files.ModName);
    }

    public async Task<string> CheckoutModsSharedBranch()
    {
        var repo = files.ModsShared.FullName;
        if (!await IsRepositoryValid(repo))
            throw new CommandException($"Invalid repository {repo}");

        var branches = await GetReposBranches(repo);
        var branch = branches.FirstOrDefault(b => b.FriendlyName == files.ModName);
        branch ??= branches.FirstOrDefault(b => b.FriendlyName == "master");
        
        if (branch == null)
            throw new CommandException("Invalid ModsShared repository branches");

        var worktree = await GetCurrentWorktree();
        if (worktree.Branch == branch.Name)
            return branch.FriendlyName;

        if (await IsGitRepoInvalidOrDirty(files.ModsShared))
            throw new CommandException(CommandCode.RepositoryIsDirty, "ModsShared Repository is dirty");

        if(!await Checkout(repo, branch.Name))
            throw new CommandException($"Could not checkout {branch.FriendlyName}");
        return branch.FriendlyName;
    }

    public async Task<bool> IsModsSharedBranchValid()
    {
        if (!await IsRepositoryValid(files.ModsShared.FullName))
            return false;

        var repo = await GetCurrentWorktree();
        return repo.Name == files.ModName ||
               (!await HasDedicatedModsSharedBranch() && repo.Name == "master");
    }

    public async Task<tot_lib.Git.Models.Worktree> GetCurrentWorktree()
    {
        var query = new Worktree(files.ModsShared.FullName);
        var results = await Task.Run(query.List);
        if(results.Count == 0)
            throw new CommandException("No worktree found");
        if(results.Count > 1)
            throw new CommandException("Multiple worktrees is unsupported");
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
        return await IsRepositoryValid(files.ModsShared.FullName);
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