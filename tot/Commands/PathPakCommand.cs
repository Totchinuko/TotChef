using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathPakCommand(KitchenFiles files, IColoredConsole console) : IInvokableCommand<PathPakCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<PathPakCommand>("pak", "Print out the path of a mod pak file")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("mod-name").AddSetter((c, v) => c.ModName = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    public string ModName { get; set; } = string.Empty;
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            console.Write(files.ModPakFile.PosixFullName());
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}