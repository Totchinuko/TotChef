using System.CommandLine;
using System.CommandLine.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CheckoutCommand : ITotCommand, ITotCommandInvoked, ITotCommandOptions
{
    public string Command => "checkout";
    public string Description => "Checkout the dedicated mod branch (sharing the same name) in the ModsShared folder";

    public string ModName { get; set; } = string.Empty;
    
    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x); 
    }

    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var git = provider.GetRequiredService<GitHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();
        var kFiles = provider.GetRequiredService<KitchenFiles>();

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
