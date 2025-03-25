using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class ListCommand : ITotCommand
{
    public string Command => "list";
    public string Description => "List the mods available in the DevKit";

    public Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        
        foreach (var directory in kFiles.ModsFolder.GetDirectories())
            console.WriteLine(directory.Name);
        return Task.FromResult(0);
    }
}
