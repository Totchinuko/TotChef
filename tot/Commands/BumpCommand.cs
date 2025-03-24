using System.CommandLine;
using System.Text.RegularExpressions;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class BumpCommand : ModBasedCommand<BumpCommandOptions, BumpCommandHandler>
{
    public BumpCommand() : base("bump", "Bump a mod version")
    {
        var arg = new Argument<string>("version-part", "Part of the version to bump (major.minor.build)");
        arg.SetDefaultValue("build");
        AddArgument(arg);
    }
}

public class BumpCommandOptions : ModBasedCommandOptions
{
    public string VersionPart { get; set; } = "build";
}

public partial class BumpCommandHandler(IConsole console, KitchenFiles kitchenFiles, GitHandler git)
    : ModBasedCommandHandler<BumpCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(BumpCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            var modInfos = await _kitchenFiles.GetModInfos();
            switch (options.VersionPart)
            {
                case "major":
                    modInfos.VersionMajor++;
                    modInfos.VersionMinor = 0;
                    modInfos.VersionBuild = 0;
                    break;
                case "minor":
                    modInfos.VersionMinor++;
                    modInfos.VersionBuild = 0;
                    break;
                case "build":
                    modInfos.VersionBuild++;
                    break;
                default:
                    throw CommandCode.MissingArg("version-part");
            }

            var regex = TitleVersionRegex();
            modInfos.Name = regex.Replace(modInfos.Name,
                $"{modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
            console.WriteLine(modInfos.Name);
            await _kitchenFiles.SetModInfos(modInfos);
            git.CommitFile(_kitchenFiles.ModFolder, _kitchenFiles.ModInfo,
                $"Bump version to {modInfos.VersionMajor}.{modInfos.VersionMinor}.{modInfos.VersionBuild}");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }

    [GeneratedRegex(@"([0-9]+)\.([0-9]+)\.([0-9]+)")]
    private static partial Regex TitleVersionRegex();
}