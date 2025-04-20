using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class DevKitCommand(GitHandler git, ILogger<DevKitCommand> logger, KitchenFiles files) : IInvokableCommand<DevKitCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<DevKitCommand>("devkit", "Open the devkit for the targeted mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            var branch = await git.CheckoutModsSharedBranch();
            logger.LogInformation("{branch} branch is now active on Shared repository", branch);
            files.DeleteAnyActive();
            files.CreateActive();
            logger.LogInformation("Set {mod} as active", files.ModName);

            Process.Start(files.Ue4Editor.FullName,
                string.Join(" ", files.UProject.FullName, string.Join(" ", Constants.EditorArgs)));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to open devkit");
            return ex.GetErrorCode();
        }

        return 0;
    }
}