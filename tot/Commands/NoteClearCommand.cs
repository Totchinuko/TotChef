using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class NoteClearCommand(PatchHandler handler, IColoredConsole console) : IInvokableCommand<NoteClearCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<NoteClearCommand>("clear", "Clear the current patch note")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            handler.DeleteCurrentPatchNote();
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}