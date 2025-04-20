using System.CommandLine;
using System.CommandLine.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class CheckoutCommand(GitHandler git, IColoredConsole console, KitchenFiles kFiles) : IInvokableCommand<CheckoutCommand>
{
    public static Command Command = CommandBuilder.CreateInvokable<CheckoutCommand>(
            "checkout", "Checkout the dedicated mod branch (sharing the same name) in the ModsShared folder")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();

    public string ModName { get; set; } = string.Empty;
    
    public async Task<int> InvokeAsync(CancellationToken cancellationToken)
    {
        try
        {
            kFiles.SetModName(ModName);
            
            if (!await git.IsModsSharedRepositoryValid())
            {
                console.Error.WriteLine("ModsShared repository is invalid");
                return CommandCode.RepositoryInvalid;
            }
            
            var branch = await git.CheckoutModsSharedBranch();
            Console.WriteLine($"{branch} branch is now active on Shared repository");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }


}
