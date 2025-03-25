using System.CommandLine;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public partial class VersionCommand : ModBasedCommand, ITotCommand, ITotCommandArguments
{
    public string VersionPart { get; protected set; } = string.Empty;
    
    public string Command => "version";
    public string Description => "Handle mod version modification and display";
    
    public IEnumerable<Argument> GetArguments()
    {
        var arg = new TotArgument<string>("version-part", "Part of the version to bump (major.minor.build)");
        arg.SetDefaultValue("build");
        arg.AddSetter(v => VersionPart = v ?? string.Empty);
        yield return arg;
    }
    
    public override async Task<int> InvokeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var kitchenFiles = services.GetRequiredService<KitchenFiles>();
        var git = services.GetRequiredService<GitHandler>();
        var console = services.GetRequiredService<IColoredConsole>();

        try
        {
            await base.InvokeAsync(services, cancellationToken);

            var modInfos = await kitchenFiles.GetModInfos();
            switch (VersionPart)
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
            await kitchenFiles.SetModInfos(modInfos);
            git.CommitFile(kitchenFiles.ModFolder, kitchenFiles.ModInfo,
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