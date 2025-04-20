using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class OpenCommand(IColoredConsole console, KitchenFiles files) : IInvokableCommand<OpenCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<OpenCommand>("open", "Open the folder containing the pak files")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            Process.Start("explorer.exe", files.ModPakFolder.FullName);
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}