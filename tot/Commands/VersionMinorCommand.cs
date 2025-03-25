using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class VersionMinorCommand : ITotCommand, ITotCommandInvoked, ITotCommandOptions
{
    public string Command => "minor";
    public string Description => "Increment the minor version";
    
    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var git = provider.GetRequiredService<GitHandler>();
        var console = provider.GetRequiredService<IColoredConsole>();

        try
        {
            kFiles.SetModName(ModName);

            var modInfos = await kFiles.GetModInfos();

            modInfos.VersionMinor++;
            modInfos.VersionBuild = 0;

            var regex = VersionCommand.TitleVersionRegex();
            modInfos.Name = regex.Replace(modInfos.Name,
                $"{modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
            console.WriteLine(modInfos.Name);
            await kFiles.SetModInfos(modInfos);
            git.CommitFile(kFiles.ModFolder, kFiles.ModInfo,
                $"Bump version to {modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}