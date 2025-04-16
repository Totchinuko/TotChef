using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class NoteAddImprovementCommand : ITotCommand, ITotCommandInvoked
{
    public string Command => "improvement";
    public string Description => "Add an improvement to the patch note";
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var config = provider.GetRequiredService<Config>();
        var handler = provider.GetRequiredService<PatchHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();
        
        try
        {
            var tmpFile = Path.GetTempFileName();
            await config.EditWithCli(tmpFile, token);
            var content = await File.ReadAllTextAsync(tmpFile, token);
            File.Delete(tmpFile);
            var note = await handler.GetCurrentPatchNote();
            string[] lines = content.Trim().Split(
                ["\r\n", "\r", "\n"],
                StringSplitOptions.RemoveEmptyEntries
            );
            note.Improvements.AddRange(lines);
            await handler.SetCurrentPatchNote(note);
            return 0;
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
    }
}