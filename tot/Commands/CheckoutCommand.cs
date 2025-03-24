using System.CommandLine;
using System.CommandLine.IO;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CheckoutCommand : Command<ModBasedCommandOptions, CheckoutCommandHandler>
{
    public CheckoutCommand() : base("checkout",
        "Checkout the dedicated mod branch (sharing the same name) in the ModsShared folder")

    {
    }
}

public class CheckoutCommandHandler(IConsole console, GitHandler git, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<ModBasedCommandOptions>(kitchenFiles)
{
    public override async Task<int> HandleAsync(ModBasedCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);


        if (!git.IsModsSharedRepositoryValid())
        {
            console.Error.WriteLine("ModsShared repository is invalid");
            return CommandCode.RepositoryInvalid;
        }

        try
        {
            git.CheckoutModsSharedBranch(out var branch);
            Console.WriteLine($"{branch} branch is now active on Shared repository");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}