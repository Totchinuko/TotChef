using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PathSharedCommand : ITotCommand, ITotCommandInvoked
{
    public string Command => "shared";
    public string Description => "Print out the path of a mod shared directory";

    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        try
        {
            console.Write(kFiles.ModsShared.PosixFullName());
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }

}