using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class DevKitCommand(GitHandler git, IColoredConsole console, KitchenFiles files) : IInvokableCommand<DevKitCommand>
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
            console.WriteLine($"{branch} branch is now active on Shared repository");
            files.DeleteAnyActive();
            files.CreateActive();
            console.WriteLine($"Set {files.ModName} as active");

            Process.Start(files.Ue4Editor.FullName,
                string.Join(" ", files.UProject.FullName, string.Join(" ", Constants.EditorArgs)));
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}