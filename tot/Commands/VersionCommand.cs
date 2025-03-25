using System.CommandLine;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public partial class VersionCommand : ITotCommandInvoked, ITotCommand, ITotCommandSubCommands, ITotCommandOptions
{
    public string VersionPart { get; protected set; } = string.Empty;
    
    public string Command => "version";
    public string Description => "Handle mod version modification and display";
    
    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public IEnumerable<ITotCommand> GetSubCommands()
    {
        yield return new VersionBuildCommand();
        yield return new VersionMinorCommand();
        yield return new VersionMajorCommand();
    }
    
   public async Task<int> InvokeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var kFiles = services.GetRequiredService<KitchenFiles>();
        var console = services.GetRequiredService<IColoredConsole>();

        try
        {
            kFiles.SetModName(ModName);
            var modInfos = await kFiles.GetModInfos();
            console.Write($"{modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }

    [GeneratedRegex(@"([0-9]+)\.([0-9]+)\.([0-9]+)")]
    public static partial Regex TitleVersionRegex();
}