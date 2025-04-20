using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class SwitchCommand(KitchenFiles files, IColoredConsole console) : IInvokableCommand<SwitchCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<SwitchCommand>("switch", "Switch the active.txt to the selected mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            files.DeleteAnyActive();
            files.CreateActive();
            console.WriteLine($"{files.ModName} is now active");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}