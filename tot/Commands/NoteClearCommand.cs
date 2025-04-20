using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class NoteClearCommand(PatchHandler handler, ILogger<NoteClearCommand> logger) : IInvokableCommand<NoteClearCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<NoteClearCommand>("clear", "Clear the current patch note")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            handler.DeleteCurrentPatchNote();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to clear note");
            return Task.FromResult(ex.GetErrorCode());
        }

        return Task.FromResult(0);
    }
}