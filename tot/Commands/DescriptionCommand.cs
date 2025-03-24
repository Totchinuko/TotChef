using System.CommandLine;
using System.Diagnostics;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class DescriptionCommand : ModBasedCommand<DescriptionCommandOptions, DescriptionCommandHandler>
{
    public DescriptionCommand() : base("description", "Edit the mod description")
    {
    }
}

public class DescriptionCommandOptions : ModBasedCommandOptions
{
}

public class DescriptionCommandHandler(Config config, IConsole console, GitHandler git, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<DescriptionCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(DescriptionCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            var modInfos = await _kitchenFiles.GetModInfos();
            var tmpFile = await _kitchenFiles.CreateTemporaryTextFile(modInfos.Description);
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
            await _kitchenFiles.SetModInfos(modInfos);
            git.CommitFile(_kitchenFiles.ModFolder, _kitchenFiles.ModCookInfo, "Update mod description");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}