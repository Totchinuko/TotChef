using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class DescriptionCommand(KitchenFiles files, Config config, IColoredConsole console, GitHandler git) : IInvokableCommand<DescriptionCommand>
{
    public static Command Command = CommandBuilder
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
            console.WriteLine("Commiting changes");
            await files.SetModInfos(modInfos);
            await git.CommitFile(files.ModFolder, files.ModCookInfo, Constants.GitCommitDescriptionMessage);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}