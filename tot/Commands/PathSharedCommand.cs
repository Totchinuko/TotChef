using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathSharedCommand(KitchenFiles files, IColoredConsole console) : IInvokableCommand<PathSharedCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<PathSharedCommand>("shared", "Print out the path of a mod shared directory")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            console.Write(files.ModsShared.PosixFullName());
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }

}