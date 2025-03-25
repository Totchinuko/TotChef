using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class SwitchCommand : ITotCommandInvoked, ITotCommandOptions, ITotCommand
{
    public string Command => "switch";
    public string Description => "Switch the active.txt to the selected mod";

    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        
        try
        {
            kFiles.SetModName(ModName);

            kFiles.DeleteAnyActive();
            kFiles.CreateActive();
            console.WriteLine($"{kFiles.ModName} is now active");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}