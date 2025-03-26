using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class DescriptionCommand : ITotCommandInvoked, ITotCommand, ITotCommandOptions
{
    public string Command => "description";
    public string Description => "Edit the mod description";

    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var config = provider.GetRequiredService<Config>();
        var console = provider.GetRequiredService<IColoredConsole>();
        var git = provider.GetRequiredService<GitHandler>();
        
        try
        {
            kFiles.SetModName(ModName);
            var modInfos = await kFiles.GetModInfos();
            var tmpFile = await kFiles.CreateTemporaryTextFile(modInfos.Description);
            using (var fileOpener = new Process())
            {
                fileOpener.StartInfo.FileName = config.DefaultCliEditor;
                fileOpener.StartInfo.Arguments = $"\"{tmpFile}\"";
                fileOpener.StartInfo.UseShellExecute = false;
                fileOpener.Start();
                await fileOpener.WaitForExitAsync(cancellationToken);
            }

            var description = await File.ReadAllTextAsync(tmpFile, cancellationToken);
            description = description.Trim();
            if (modInfos.Description == description)
                return 0;

            modInfos.Description = description;
            console.WriteLine("Commiting changes");
            await kFiles.SetModInfos(modInfos);
            await git.CommitFile(kFiles.ModFolder, kFiles.ModCookInfo, Constants.GitCommitDescriptionMessage);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}