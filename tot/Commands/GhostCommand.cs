using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class GhostCommand(ILogger<GhostCommand> logger, KitchenFiles files) : IInvokableCommand<GhostCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<GhostCommand>("ghost", "List invalid files and clean them")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c,v) => c.ModName = v)
        .Options.Create<bool>("--cleanup").AddAlias("-c")
        .AddSetter((c,v) => c.Cleanup = v).BuildOption()
        .BuildCommand();
    
    public bool Cleanup { get; set; }
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            List<string> fileList = Directory.GetFiles(files.ModFolder.FullName, "*.*", SearchOption.AllDirectories)
                .ToList();

            logger.LogInformation("Invalid files:");
            foreach (var file in fileList)
            {
                var info = new FileInfo(file);
                if (info.Extension != Constants.UAssetExt && info.Extension != Constants.UMapExt) continue;
                if (info.Length >= 4 * 1024) continue;
                var contain = await files.FileContain(info, "ObjectRedirector");
                if (!contain && info.Length >= 1024) continue;
                logger.LogInformation("{file}", file);
                if (Cleanup)
                    info.Delete();
            }
        }
        catch(Exception ex)
        {
            logger.LogCritical(ex, "Failed to scan files");
            return ex.GetErrorCode();
        }
        
        return 0;
    }
}
