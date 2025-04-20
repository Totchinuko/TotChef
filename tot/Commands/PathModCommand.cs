using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathModCommand(KitchenFiles files, IColoredConsole console) : IInvokableCommand<PathModCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<PathModCommand>("mod", "Print out the path of a mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("mod-name").AddSetter((c, v) => c.ModName = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    public string ModName { get; set; } = string.Empty;
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            console.Write(files.ModFolder.PosixFullName());
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}