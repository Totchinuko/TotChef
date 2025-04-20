using System.CommandLine;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ListCommand(IConsole console, KitchenFiles files) : IInvokableCommand<ListCommand>
{
    public static readonly Command Command = CommandBuilder
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
