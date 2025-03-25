using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PakCommand : ModBasedCommand, ITotCommand
{
    public string Command => "pak";
    public string Description => "Pak the previously cooked files";
    public bool Compress { get; set; }

    public override IEnumerable<Option> GetOptions()
    {
        foreach (var option in base.GetOptions())
            yield return option;
        var opt = new TotOption<bool>("--compress", "Compress the files to reduce the final mod size");
        opt.AddAlias("-c");
        opt.AddSetter(x => Compress = x);
        yield return opt;
    }

    public override async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        await base.InvokeAsync(provider, token);
        var kFile = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();
        
        
        try
        {
            kFile.CreateModPakBackup();
            foreach (var file in kFile.ModFolder.GetFiles())
                if (!file.Name.StartsWith(".") && file.Name != "active.txt")
                    file.CopyTo(Path.Join(kFile.ModCookedFolder.FullName, file.Name), true);

            console.WriteLine($"Paking {kFile.ModName}..");
            var p = Process.Start(
                kFile.UnrealPak.FullName,
                string.Join(" ",
                    kFile.ModPakFile.FullName,
                    "-Create=" + kFile.ModCookedFolder.FullName,
                    Compress ? "-compress" : ""
                ));
            await p.WaitForExitAsync(token);
            console.WriteLine($"{kFile.ModName} has been paked successfully.. !");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}