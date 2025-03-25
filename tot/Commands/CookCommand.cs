using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CookCommand : ModBasedCommand, ITotCommand
{
    public bool Force { get; set; }
    public bool Verbose { get; set; }
    
    public string Command => "cook";
    public string Description => "Start a cook process for the mod";

    public override IEnumerable<Option> GetOptions()
    {
        foreach (var option in base.GetOptions())
            yield return option;
        
        var opt = new TotOption<bool>("--force", "Force the cook process even if the repo is dirty");
        opt.AddAlias("-f");
        opt.AddSetter(x => Force = x);
        yield return opt;
        opt = new TotOption<bool>("--verbose", "Display the Dev Kit cook output");
        opt.AddAlias("-v");
        opt.AddSetter(x => Verbose = x);
        yield return opt;
    }

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var git = provider.GetRequiredService<GitHandler>();
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        var stove = provider.GetRequiredService<Stove>();
        var clerk = provider.GetRequiredService<KitchenClerk>();

        try
        {
            await base.InvokeAsync(provider, cancellationToken);

            if (git.IsGitRepoDirty(kFiles.ModsShared) && !Force)
                throw new CommandException(CommandCode.RepositoryIsDirty, "Cooking:ModsShared repo is dirty");
            if (git.IsGitRepoDirty(kFiles.ModFolder) && !Force)
                throw new CommandException(CommandCode.RepositoryIsDirty,
                    $"Cooking:Mod {kFiles.ModName} repo is dirty");
            if (!git.IsModsSharedBranchValid())
                throw new CommandException(CommandCode.RepositoryWrongBranch,
                    "Cooking:Dedicated ModsShared branch is not checked out");
            
            kFiles.DeleteAnyActive();
            kFiles.CreateActive();
            console.WriteLine($"Cooking:{kFiles.ModName} is now active");

            var cookInfos = await clerk.GetCookInfo();
            var change = clerk.UpdateIncludedCookInfo(kFiles.ModLocalFolder, cookInfos);

            if (change.Count > 0)
            {
                await clerk.SetCookInfo(cookInfos);
                console.WriteLine($"Cooking:Added {change.Count} missing local mod files to cooking");
                foreach (var c in change)
                    console.WriteLine("Cooking:Change:" + c);
            }

            await clerk.AutoBumpBuild();
            await clerk.UpdateModDevKitVersion();
            console.WriteLine("Cooking:Cleaning cook folders from previous operations...");
            clerk.CleanCookedFolder();
            clerk.CleanCookingFolder();
            console.WriteLine($"Cooking:Cooking {kFiles.ModName}...");

            stove.StartCooking(Verbose);
            if (!stove.WasSuccess)
                throw new CommandException(CommandCode.CookingFailure, $"Cooking failed. {stove.Errors} Error(s)");

            await clerk.CopyAndFilter(Verbose);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        console.WriteLine($"{kFiles.ModName} cooked successfully.. !");
        return 0;
    }
}
