using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class SwitchCommand : ModBasedCommand, ITotCommand
{
    public string Command => "switch";
    public string Description => "Switch the active.txt to the selected mod";

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        
        try
        {
            await base.InvokeAsync(provider, token);

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