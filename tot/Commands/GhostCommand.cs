using System.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class GhostCommand : ModBasedCommand<GhostCommandOptions, GhostCommandHandler>
{
    public GhostCommand() : base("ghost", "List invalid files and clean them")
    {
        var opt = new Option<bool>("--cleanup");
        opt.AddAlias("-c");
        AddOption(opt);
    }
}

public class GhostCommandOptions : ModBasedCommandOptions
{
    public bool Cleanup { get; set; }
}

public class GhostCommandHandler(KitchenFiles kitchenFiles, IConsole console)
    : ModBasedCommandHandler<GhostCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(GhostCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        List<string> files = Directory.GetFiles(_kitchenFiles.ModFolder.FullName, "*.*", SearchOption.AllDirectories)
            .ToList();

        console.WriteLine("Invalid files:");
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            if (info.Extension != Constants.UAssetExt && info.Extension != Constants.UMapExt) continue;
            if (info.Length >= 4 * 1024) continue;
            var contain = await _kitchenFiles.FileContain(info, "ObjectRedirector");
            if (!contain && info.Length >= 1024) continue;
            console.WriteLine($"{file}");
            if (options.Cleanup)
                info.Delete();
        }

        return 0;
    }
}