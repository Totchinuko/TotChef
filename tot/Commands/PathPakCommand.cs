using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PathPakCommand(KitchenFiles files, IConsole console, ILogger<PathPakCommand> logger) : IInvokableCommand<PathPakCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<PathPakCommand>("pak", "Print out the path of a mod pak file")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("mod-name").SetSetter((c, v) => c.ModName = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    public string ModName { get; set; } = string.Empty;
    public Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            console.Write(files.ModPakFile.PosixFullName());
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to find pak");
            return Task.FromResult(ex.GetErrorCode());
        }
    }
}