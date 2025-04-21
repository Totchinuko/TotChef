using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class CookCommand(ILogger<CookCommand> logger,GitHandler git, KitchenFiles files, Stove stove, KitchenClerk clerk) : IInvokableCommand<CookCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<CookCommand>("cook", "Start a cook process for the mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.Create<bool>("--force", "Force the cook process even if the repo is dirty").AddAlias("-f")
        .SetSetter((c, v) => c.Force = v).BuildOption()
        .Options.Create<bool>("--verbose", "Display the Dev Kit cook output").AddAlias("-v")
        .SetSetter((c, v) => c.Verbose = v).BuildOption()
        .Options.Create<bool>("--no-version-bump", "Prevent the auto bump of the build version").AddAlias("-nv")
        .SetSetter((c, v) => c.NoVersionBump = v).BuildOption()
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();
    
    public bool Force { get; set; }
    public bool Verbose { get; set; }
    public bool NoVersionBump { get; set; }
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken cancellationToken)
    {
        try
        {
            files.SetModName(ModName);
            if (await git.IsGitRepoInvalidOrDirty(files.ModsShared) && !Force)
                throw new Exception("ModsShared repo is dirty");
            if (await git.IsGitRepoInvalidOrDirty(files.ModFolder) && !Force)
                throw new Exception($"Mod {files.ModName} repo is dirty");
            if (!await git.IsModsSharedBranchValid())
                throw new Exception("Dedicated ModsShared branch is not checked out");
            
            files.DeleteAnyActive();
            files.CreateActive();
            logger.LogInformation("{mod} is now active", files.ModName);

            var cookInfos = await clerk.GetCookInfo();
            var change = clerk.UpdateIncludedCookInfo(files.ModLocalFolder, cookInfos);

            if (change.Count > 0)
            {
                if (Force)
                    await clerk.SetCookInfo(cookInfos);
                else
                    await clerk.SetCookInfoAndCommit(cookInfos);
                logger.LogWarning("Added {changes} missing local mod files to cooking", change.Count);
                foreach (var c in change)
                    logger.LogWarning("Change:{change}",c);
            }

            if (!NoVersionBump)
            {
                await clerk.AutoBumpBuild();
                await clerk.UpdateModDevKitVersion();
            }
            logger.LogInformation("Cleaning cook folders from previous operations...");
            clerk.CleanCookedFolder();
            clerk.CleanCookingFolder();
            logger.LogInformation("Cooking {mod}...", files.ModName);

            await stove.StartCooking(cancellationToken, Verbose);
            if (!stove.WasSuccess)
                throw new Exception($"Cooking failed. {stove.Errors} Error(s)");

            await clerk.CopyAndFilter(Verbose);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Cooking failed");
            return ex.GetErrorCode();
        }

        logger.LogInformation("{mod} cooked successfully.. !", files.ModName);
        return 0;
    }
}
