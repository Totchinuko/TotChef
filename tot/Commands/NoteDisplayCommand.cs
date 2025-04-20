using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class NoteDisplayCommand(Config config, PatchHandler handler, IConsole console, ILogger<NoteDisplayCommand> logger) : IInvokableCommand<NoteDisplayCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<NoteDisplayCommand>("display", "Display the current patch note")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.Create<bool>("--in-editor", "Display in an editor like vim or nano instead of console output")
        .AddAlias("-e")
        .AddSetter((c,v) => c.DisplayInEditor = v).BuildOption()
        .BuildCommand();

    public bool DisplayInEditor { get; set; }
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            if (!await handler.PatchNoteExists())
            {
                logger.LogError("Patch note is empty");
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
            
            console.Write(builder.ToString());
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to display note");
            return ex.GetErrorCode();
        }

        return 0;
    }
}