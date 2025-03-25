using LibGit2Sharp;
using tot_lib;
using Tot;

namespace tot.Services;

public class GitHandler(Config config, KitchenFiles files) : ITotService
{
    public void CommitFile(DirectoryInfo directory, FileInfo file, string message)
    {
        if (!Repository.IsValid(directory.FullName))
            throw new CommandException(CommandCode.RepositoryInvalid, "Invalid repository");

        using Repository repo = new(directory.FullName);
        var status = repo.RetrieveStatus();
        if (status.Staged.Any())
            throw new CommandException(CommandCode.RepositoryIsDirty, "Repository is dirty");
        repo.Index.Add(file.FullName.RemoveRootFolder(directory.FullName));
        repo.Index.Write();
        Signature author = new(config.GitAuthorName, config.GitAuthorEmail, DateTime.Now);
        repo.Commit(message, author, author);
    }

    public bool IsGitRepoDirty(DirectoryInfo directory)
    {
        if (!Repository.IsValid(directory.FullName)) return false;
        using var repo = new Repository(directory.FullName);
        return repo.RetrieveStatus().IsDirty;
    }

    public bool HasDedicatedModsSharedBranch()
    {
        if (!Repository.IsValid(files.ModsShared.FullName)) return false;
        using Repository repo = new(files.ModsShared.FullName);
        return repo.Branches.Any(branch => branch.FriendlyName == files.ModName);
    }

    public void CheckoutModsSharedBranch(out string branchName)
    {
        branchName = "master";
        if (!Repository.IsValid(files.ModsShared.FullName))
            throw CommandCode.NotFound(files.ModsShared);


        using Repository repo = new(files.ModsShared.FullName);
        Branch? branch = null;
        foreach (var b in repo.Branches)
            if (b.FriendlyName == files.ModName)
                branch = b;

        if (branch == null)
            branch = repo.Branches["master"];

        branchName = branch.FriendlyName;
        if (repo.Head.CanonicalName == branch.CanonicalName)
            return;

        if (IsGitRepoDirty(files.ModsShared))
            throw new CommandException(CommandCode.RepositoryIsDirty, "ModsShared Repository is dirty");

        Commands.Checkout(repo, branch);
    }

    public bool IsModsSharedBranchValid()
    {
        using var repo = new Repository(files.ModsShared.FullName);
        return repo.Head.FriendlyName == files.ModName ||
               (!HasDedicatedModsSharedBranch() && repo.Head.FriendlyName == "master");
    }

    public bool IsModsSharedRepositoryValid()
    {
        return Repository.IsValid(files.ModsShared.FullName);
    }
}