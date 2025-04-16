using System.CommandLine;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class NoteDisplayCommand : ITotCommand, ITotCommandOptions, ITotCommandInvoked
{
    public string Command => "display";
    public string Description => "Display the current patch note";
    public bool DisplayInEditor { get; set; }
    
    public IEnumerable<Option> GetOptions()
    {
        var displayInEditor = new TotOption<bool>("--in-editor",
            "Display in an editor like vim or nano instead of console output");
        displayInEditor.AddAlias("-e");
        displayInEditor.AddSetter((v) => DisplayInEditor = v);
        yield return displayInEditor;
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var config = provider.GetRequiredService<Config>();
        var handler = provider.GetRequiredService<PatchHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();

        try
        {
            if (!await handler.PatchNoteExists())
            {
                console.WriteLine(ConsoleColor.Red, "Patch note is empty");
                return 1;
            }
            
            var note = await handler.GetCurrentPatchNote();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("## What's Changed");
            builder.AppendLine(string.Empty);
            
            foreach (var line in note.Additions)
                builder.AppendLine($"- {line}");
            foreach (var line in note.Improvements)
                builder.AppendLine($"- {line}");
            foreach (var line in note.Fixes)
                builder.AppendLine($"- {line}");

            if (DisplayInEditor)
            {
                var tmpFile = Path.GetTempFileName();
                await File.WriteAllTextAsync(tmpFile, builder.ToString(), token);
                await config.EditWithCli(tmpFile, token);
                File.Delete(tmpFile);
                return 0;
            }
            
            console.WriteLines(builder.ToString().Split(Environment.NewLine));
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
    
}