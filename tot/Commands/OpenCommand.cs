using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class OpenCommand : ModBasedCommand, ITotCommand
{
    public string Command => "open";
    public string Description => "Open the folder containing the pak files";

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IConsole>();
        
        try
        {
            await base.InvokeAsync(provider, token);
            Process.Start("explorer.exe", provider.GetRequiredService<KitchenFiles>().ModPakFolder.FullName);
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}