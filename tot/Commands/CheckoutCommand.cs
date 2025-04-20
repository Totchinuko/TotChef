using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class CheckoutCommand(GitHandler git, ILogger<CheckoutCommand> logger, KitchenFiles kFiles) : IInvokableCommand<CheckoutCommand>
{
    public static readonly Command Command = CommandBuilder.CreateInvokable<CheckoutCommand>(
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
                logger.LogError("ModsShared repository is invalid");
                return 1;
            }
            
            var branch = await git.CheckoutModsSharedBranch();
            Console.WriteLine($"{branch} branch is now active on Shared repository");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Checkout Failed");
            return ex.GetErrorCode();
        }

        return 0;
    }


}
