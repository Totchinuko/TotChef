using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class SwitchCommand(KitchenFiles files, ILogger<SwitchCommand> logger) : IInvokableCommand<SwitchCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<SwitchCommand>("switch", "Switch the active.txt to the selected mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            files.DeleteAnyActive();
            files.CreateActive();
            logger.LogInformation("{mod} is now active", files.ModName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to switch");
            return Task.FromResult(ex.GetErrorCode());
        }

        return Task.FromResult(0);
    }
}