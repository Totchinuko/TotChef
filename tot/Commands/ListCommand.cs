using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ListCommand(IColoredConsole console, KitchenFiles files) : IInvokableCommand<ListCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<ListCommand>("list", "List the mods available in the DevKit")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        foreach (var directory in files.ModsFolder.GetDirectories())
            console.WriteLine(directory.Name);
        return Task.FromResult(0);
    }
}
