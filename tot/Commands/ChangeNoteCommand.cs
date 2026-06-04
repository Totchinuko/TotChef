using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ChangeNoteCommand(KitchenFiles files, Config config, ILogger<ChangeNoteCommand> logger, GitHandler git) : IInvokableCommand<ChangeNoteCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<ChangeNoteCommand>("change-note", "Edit the mod patch node")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken cancellationToken)
    {
        try
        {
            files.SetModName(ModName);
            var modInfos = await files.GetModInfos();
            var tmpFile = await files.CreateTemporaryTextFile(modInfos.ChangeNote);
            await config.EditWithCli(tmpFile, cancellationToken);

            var changeNote = await File.ReadAllTextAsync(tmpFile, cancellationToken);
            File.Delete(tmpFile);
            changeNote = changeNote.Trim();
            if (modInfos.ChangeNote == changeNote)
                return 0;

            modInfos.ChangeNote = changeNote;
            logger.LogInformation("Commiting changes");
            await files.SetModInfos(modInfos);
            await git.CommitFile(files.ModFolder, files.ModInfo, Constants.GitCommitChangeNoteMessage);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Change Note edit failed");
            return ex.GetErrorCode();
        }

        return 0;
    }
}