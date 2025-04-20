using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class DescriptionCommand(KitchenFiles files, Config config, ILogger<DescriptionCommand> logger, GitHandler git) : IInvokableCommand<DescriptionCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<DescriptionCommand>("description", "Edit the mod description")
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
            var tmpFile = await files.CreateTemporaryTextFile(modInfos.Description);
            await config.EditWithCli(tmpFile, cancellationToken);

            var description = await File.ReadAllTextAsync(tmpFile, cancellationToken);
            File.Delete(tmpFile);
            description = description.Trim();
            if (modInfos.Description == description)
                return 0;

            modInfos.Description = description;
            logger.LogInformation("Commiting changes");
            await files.SetModInfos(modInfos);
            await git.CommitFile(files.ModFolder, files.ModCookInfo, Constants.GitCommitDescriptionMessage);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Description edit failed");
            return ex.GetErrorCode();
        }

        return 0;
    }
}