using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class DevKitCommand : ITotCommandInvoked, ITotCommandOptions, ITotCommand
{
    public string Command => "devkit";
    public string Description => "Open the devkit for the targeted mod";

    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var git = provider.GetRequiredService<GitHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        
        try
        {
            kFiles.SetModName(ModName);

            var branch = await git.CheckoutModsSharedBranch();
            console.WriteLine($"{branch} branch is now active on Shared repository");
            kFiles.DeleteAnyActive();
            kFiles.CreateActive();
            console.WriteLine($"Set {kFiles.ModName} as active");

            Process.Start(kFiles.Ue4Editor.FullName,
                string.Join(" ", kFiles.UProject.FullName, string.Join(" ", Constants.EditorArgs)));
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}