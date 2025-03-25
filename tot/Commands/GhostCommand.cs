using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class GhostCommand : ModBasedCommand, ITotCommand, ITotCommandOptions
{
    public string Command => "ghost";
    public string Description => "List invalid files and clean them";
    
    public bool Cleanup { get; set; }

    public override IEnumerable<Option> GetOptions()
    {
        foreach (var option in base.GetOptions())
            yield return option;
        var opt = new TotOption<bool>("--cleanup");
        opt.AddAlias("-c");
        opt.AddSetter(x => Cleanup = x);
        yield return opt;
    }

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        await base.InvokeAsync(provider, token);
        var console = provider.GetRequiredService<IColoredConsole>();
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        
        List<string> files = Directory.GetFiles(kFiles.ModFolder.FullName, "*.*", SearchOption.AllDirectories)
            .ToList();

        console.WriteLine("Invalid files:");
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            if (info.Extension != Constants.UAssetExt && info.Extension != Constants.UMapExt) continue;
            if (info.Length >= 4 * 1024) continue;
            var contain = await kFiles.FileContain(info, "ObjectRedirector");
            if (!contain && info.Length >= 1024) continue;
            console.WriteLine($"{file}");
            if (Cleanup)
                info.Delete();
        }

        return 0;
    }
}
