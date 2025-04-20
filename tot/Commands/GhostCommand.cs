using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class GhostCommand(IColoredConsole console, KitchenFiles files) : IInvokableCommand<GhostCommand>
{
    public static Command Command = CommandBuilder
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

            console.WriteLine("Invalid files:");
            foreach (var file in fileList)
            {
                var info = new FileInfo(file);
                if (info.Extension != Constants.UAssetExt && info.Extension != Constants.UMapExt) continue;
                if (info.Length >= 4 * 1024) continue;
                var contain = await files.FileContain(info, "ObjectRedirector");
                if (!contain && info.Length >= 1024) continue;
                console.WriteLine($"{file}");
                if (Cleanup)
                    info.Delete();
            }
        }
        catch(CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }
        
        return 0;
    }
}
