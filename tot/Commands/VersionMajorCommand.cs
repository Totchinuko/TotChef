using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class VersionMajorCommand(KitchenFiles files, GitHandler git, IColoredConsole console) : IInvokableCommand<VersionMajorCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<VersionMajorCommand>("major", "Increment the major version")
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

            modInfos.VersionMajor++;
            modInfos.VersionMinor = 0;
            modInfos.VersionBuild = 0;

            var regex = VersionCommand.TitleVersionRegex();
            modInfos.Name = regex.Replace(modInfos.Name,
                $"{modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
            console.WriteLine(modInfos.Name);
            await files.SetModInfos(modInfos);
            await git.CommitFile(files.ModFolder, files.ModInfo,
                string.Format(
                    Constants.GitCommitVersionMessage, 
                    modInfos.VersionMajor, modInfos.VersionMinor, modInfos.VersionBuild));
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}