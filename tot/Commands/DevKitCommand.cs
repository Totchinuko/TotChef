using System.CommandLine;
using System.Diagnostics;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class DevKitCommand : ModBasedCommand<DevKitCommandOptions, DevKitCommandHandler>
{
    public DevKitCommand() : base("devkit", "Open the devkit for the targeted mod")
    {
    }
}

public class DevKitCommandOptions : ModBasedCommandOptions
{
}

public class DevKitCommandHandler(IConsole console, KitchenFiles kitchenFiles, GitHandler git)
    : ModBasedCommandHandler<DevKitCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(DevKitCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            git.CheckoutModsSharedBranch(out var branch);
            console.WriteLine($"{branch} branch is now active on Shared repository");
            _kitchenFiles.DeleteAnyActive();
            _kitchenFiles.CreateActive();
            console.WriteLine($"Set {_kitchenFiles.ModName} as active");

            Process.Start(_kitchenFiles.Ue4Editor.FullName,
                string.Join(" ", _kitchenFiles.UProject.FullName, string.Join(" ", Constants.EditorArgs)));
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}