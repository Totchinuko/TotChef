using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathSharedCommand(KitchenFiles files, IConsole console, ILogger<PathSharedCommand> logger) : IInvokableCommand<PathSharedCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<PathSharedCommand>("shared", "Print out the path of a mod shared directory")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            console.Write(files.ModsShared.PosixFullName());
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to find mod");
            return Task.FromResult(ex.GetErrorCode());
        }
    }

}