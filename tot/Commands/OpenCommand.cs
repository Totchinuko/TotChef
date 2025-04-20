using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class OpenCommand(ILogger<OpenCommand> logger, KitchenFiles files) : IInvokableCommand<OpenCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<OpenCommand>("open", "Open the folder containing the pak files")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            Process.Start("explorer.exe", files.ModPakFolder.FullName);
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to open");
            return Task.FromResult(ex.GetErrorCode());
        }
    }
}