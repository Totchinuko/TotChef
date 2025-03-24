using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CookCommand : Command<CookCommandOptions, CookCommandHandler>
{
    public CookCommand() : base("cook", "Start a cook process for the mod")
    {
        var opt = new Option<bool>("--force", "Force the cook process even if the repo is dirty");
        opt.AddAlias("-f");
        AddOption(opt);
        opt = new Option<bool>("--verbose", "Display the Dev Kit cook output");
        opt.AddAlias("-v");
        AddOption(opt);
    }
}

public class CookCommandOptions : ModBasedCommandOptions
{
    public bool Force { get; set; }
    public bool Verbose { get; set; }
}

public class CookCommandHandler(
    IConsole console,
    Stove stove,
    KitchenClerk clerk,
    GitHandler git,
    KitchenFiles kitchenFiles) : ModBasedCommandHandler<CookCommandOptions>(kitchenFiles)
{
    private readonly IConsole _console = console;
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(CookCommandOptions options, CancellationToken cancellationToken)
    {
        var code = await base.HandleAsync(options, cancellationToken);
        if (code != 0) return code;

        if (git.IsGitRepoDirty(_kitchenFiles.ModsShared) && !options.Force)
            return await _console.OutputCommandError(CommandCode.RepositoryIsDirty, "Cooking:ModsShared repo is dirty");
        if (git.IsGitRepoDirty(_kitchenFiles.ModFolder) && !options.Force)
            return await _console.OutputCommandError(CommandCode.RepositoryIsDirty,
                $"Cooking:Mod {_kitchenFiles.ModName} repo is dirty");
        if (!git.IsModsSharedBranchValid())
            return await _console.OutputCommandError(CommandCode.RepositoryWrongBranch,
                "Cooking:Dedicated ModsShared branch is not checked out");

        try
        {
            _kitchenFiles.DeleteAnyActive();
            _kitchenFiles.CreateActive();
            _console.WriteLine($"Cooking:{_kitchenFiles.ModName} is now active");

            var cookInfos = await clerk.GetCookInfo();
            var change = clerk.UpdateIncludedCookInfo(_kitchenFiles.ModLocalFolder, cookInfos);

            if (change.Count > 0)
            {
                await clerk.SetCookInfo(cookInfos);
                _console.WriteLine($"Cooking:Added {change.Count} missing local mod files to cooking");
                foreach (var c in change)
                    _console.WriteLine("Cooking:Change:" + c);
            }

            await clerk.AutoBumpBuild();
            await clerk.UpdateModDevKitVersion();
            _console.WriteLine("Cooking:Cleaning cook folders from previous operations...");
            clerk.CleanCookedFolder();
            clerk.CleanCookingFolder();
            _console.WriteLine($"Cooking:Cooking {_kitchenFiles.ModName}...");

            stove.StartCooking(options.Verbose);
            if (!stove.WasSuccess)
                throw new CommandException(CommandCode.CookingFailure, $"Cooking failed. {stove.Errors} Error(s)");

            await clerk.CopyAndFilter(options.Verbose);
        }
        catch (CommandException ex)
        {
            return await _console.OutputCommandError(ex);
        }

        _console.WriteLine($"{_kitchenFiles.ModName} cooked successfully.. !");
        return 0;
    }
}