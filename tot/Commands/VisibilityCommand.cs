using System.CommandLine;
using System.Xml;
using Microsoft.Extensions.Logging;
using Pastel;
using Steamworks;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class VisibilityCommand(IConsole console, GitHandler git, KitchenFiles files, ILogger<VisibilityCommand> logger) : IInvokableCommand<VisibilityCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<VisibilityCommand>("visibility", "Change the visibility of the mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .Options.Create<int>("--visibility", "Set the visibility directly, skip the manual input").AddAlias("-v")
        .SetDefault(-1)
        .SetSetter((c, v) => c.Visibility = v).BuildOption()
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;
    public int Visibility { get; set; } = -1;
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            if (Visibility < 0)
            {
                console.WriteLine("Visibility Options:");
                foreach (var value in Enum.GetValues<ERemoteStoragePublishedFileVisibility>())
                {
                    console.WriteLine($"{(int)value} - {Enum.GetName(value)}");
                }

                return 0;
            }

            files.SetModName(ModName);
            var infos = await files.GetModInfos();
            if (!Enum.IsDefined(typeof(ERemoteStoragePublishedFileVisibility), Visibility))
                throw new Exception("Visibility value is invalid");
            infos.SteamVisibility = Visibility;
            await files.SetModInfos(infos);
            await git.CommitFile(files.ModFolder, files.ModInfo, Constants.GitCommitVisibilityMessage);
            logger.LogInformation("Updated visibility");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to change visibility");
            return ex.GetErrorCode();
        }
        return 0;
    }
}
