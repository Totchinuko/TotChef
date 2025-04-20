using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathModCommand(KitchenFiles files, IConsole console, ILogger<PathModCommand> logger) : IInvokableCommand<PathModCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<PathModCommand>("mod", "Print out the path of a mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("mod-name").AddSetter((c, v) => c.ModName = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    public string ModName { get; set; } = string.Empty;
    public Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            console.Write(files.ModFolder.PosixFullName());
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to find mod");
            return Task.FromResult(ex.GetErrorCode());
        }
    }
}