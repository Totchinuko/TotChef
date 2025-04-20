using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class VersionMinorCommand(KitchenFiles files, GitHandler git, ILogger<VersionMinorCommand> logger) : IInvokableCommand<VersionMinorCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<VersionMinorCommand>("minor", "Increment the minor version")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            var modInfos = await files.GetModInfos();

            modInfos.VersionMinor++;
            modInfos.VersionBuild = 0;

            var regex = VersionCommand.TitleVersionRegex();
            modInfos.Name = regex.Replace(modInfos.Name,
                $"{modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
            logger.LogInformation(modInfos.Name);
            await files.SetModInfos(modInfos);
            await git.CommitFile(files.ModFolder, files.ModInfo,
                string.Format(
                    Constants.GitCommitVersionMessage, 
                    modInfos.VersionMajor, modInfos.VersionMinor, modInfos.VersionBuild));
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to change version");
            return ex.GetErrorCode();
        }

        return 0;
    }
}