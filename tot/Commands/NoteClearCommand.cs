using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class NoteClearCommand : ITotCommand, ITotCommandInvoked
{
    public string Command => "clear";
    public string Description => "Clear the current patch note";
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var config = provider.GetRequiredService<Config>();
        var handler = provider.GetRequiredService<PatchHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();

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