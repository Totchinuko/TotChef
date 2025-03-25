using System.CommandLine;
using System.CommandLine.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CheckoutCommand : ITotCommand, ITotCommandInvoked
{
    public string Command => "checkout";
    public string Description => "Checkout the dedicated mod branch (sharing the same name) in the ModsShared folder";

    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var git = provider.GetRequiredService<GitHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();

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
