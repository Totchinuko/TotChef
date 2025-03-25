using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PathCommand : ModBasedCommand, ITotCommand
{
    public string Command => "path";
    public string Description => "Return a path to be used with cd";
    
    public bool ModsShared { get; set; }
    public bool PakFile { get; set; }

    public override IEnumerable<Option> GetOptions()
    {
        foreach (var option in base.GetOptions())
            yield return option;
        var opt = new TotOption<bool>("--mods-shared", "Return the path to the shared folder");
        opt.AddAlias("-s");
        opt.AddSetter(x => ModsShared = x);
        yield return opt;
        
        opt = new TotOption<bool>("--pak-file", "Return the path to the cooked pak file");
        opt.AddAlias("-p");
        opt.AddSetter(x => PakFile = x);
        yield return opt;
    }

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        await base.InvokeAsync(provider, token);
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        
        try
        {
            var path = "";
            if (!string.IsNullOrEmpty(kFiles.ModName))
            {
                if (!kFiles.IsModPathValid())
                    throw CommandCode.NotFound(kFiles.ModFolder);
                path = kFiles.ModFolder.PosixFullName();
            }
            else
            {
                if (ModsShared)
                    path = kFiles.ModsShared.PosixFullName();
                else if (PakFile)
                    path = kFiles.ModPakFile.PosixFullName();
                else
                    throw CommandCode.MissingArg(nameof(ConanMod));
            }

            console.Write(path);
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}

