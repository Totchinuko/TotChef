using System.CommandLine;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class SwitchCommand : ModBasedCommand<SwitchCommandOptions, SwitchCommandHandler>
{
    public SwitchCommand() : base("switch", "Switch the active.txt to the selected mod")
    {
    }
}

public class SwitchCommandOptions : ModBasedCommandOptions
{
}

public class SwitchCommandHandler(IConsole console, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<SwitchCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(SwitchCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            _kitchenFiles.DeleteAnyActive();
            _kitchenFiles.CreateActive();
            console.WriteLine($"{_kitchenFiles.ModName} is now active");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}