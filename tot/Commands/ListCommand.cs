using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class ListCommand : Command<ListCommandOptions, ListCommandHandler>
{
    public ListCommand() : base("list", "List the mods available in the DevKit")
    {
    }
}

public class ListCommandOptions : ICommandOptions
{
}

public class ListCommandHandler(IConsole console, KitchenFiles kitchenFiles)
    : ICommandOptionsHandler<ListCommandOptions>
{
    public Task<int> HandleAsync(ListCommandOptions options, CancellationToken cancellationToken)
    {
        foreach (var directory in kitchenFiles.ModsFolder.GetDirectories())
            console.WriteLine(directory.Name);
        return Task.FromResult(0);
    }
}