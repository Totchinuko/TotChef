using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class VersionBuildCommand(KitchenFiles files, GitHandler git, ILogger<VersionBuildCommand> logger) : IInvokableCommand<VersionBuildCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<VersionBuildCommand>("build", "Increment the build version")
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
            
            modInfos.VersionBuild++;

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