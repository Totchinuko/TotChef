using System.CommandLine;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public partial class VersionCommand(KitchenFiles files, ILogger<VersionCommand> logger) : IInvokableCommand<VersionCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<VersionCommand>("version", "Handle mod version modification and display")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .SubCommands.Add(VersionMajorCommand.Command)
        .SubCommands.Add(VersionMinorCommand.Command)
        .SubCommands.Add(VersionBuildCommand.Command)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;
   public async Task<int> InvokeAsync(CancellationToken cancellationToken)
    {
        try
        {
            files.SetModName(ModName);
            var modInfos = await files.GetModInfos();
            logger.LogInformation("{major}.{minor}.{build}", 
                modInfos.VersionMajor,modInfos.VersionMinor,modInfos.VersionBuild);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to change version");
            return ex.GetErrorCode();
        }

        return 0;
    }

    [GeneratedRegex(@"([0-9]+)\.([0-9]+)\.([0-9]+)")]
    public static partial Regex TitleVersionRegex();
}