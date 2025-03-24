using System.CommandLine;
using System.Diagnostics;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class PakCommand : ModBasedCommand<PakCommandOptions, PakCommandHandler>
{
    public PakCommand() : base("pak", "Pak the previously cooked files")
    {
        var opt = new Option<bool>("--compress", "Compress the files to reduce the final mod size");
        opt.AddAlias("-c");
        AddOption(opt);
    }
}

public class PakCommandOptions : ModBasedCommandOptions
{
    public bool Compress { get; set; }
}

public class PakCommandHandler(IConsole console, KitchenFiles kitchenFiles)
    : ModBasedCommandHandler<PakCommandOptions>(kitchenFiles)
{
    private readonly KitchenFiles _kitchenFiles = kitchenFiles;

    public override async Task<int> HandleAsync(PakCommandOptions options, CancellationToken cancellationToken)
    {
        await base.HandleAsync(options, cancellationToken);

        try
        {
            _kitchenFiles.CreateModPakBackup();
            foreach (var file in _kitchenFiles.ModFolder.GetFiles())
                if (!file.Name.StartsWith(".") && file.Name != "active.txt")
                    file.CopyTo(Path.Join(_kitchenFiles.ModCookedFolder.FullName, file.Name), true);

            console.WriteLine($"Paking {_kitchenFiles.ModName}..");
            var p = Process.Start(
                _kitchenFiles.UnrealPak.FullName,
                string.Join(" ",
                    _kitchenFiles.ModPakFile.FullName,
                    "-Create=" + _kitchenFiles.ModCookedFolder.FullName,
                    options.Compress ? "-compress" : ""
                ));
            await p.WaitForExitAsync(cancellationToken);
            console.WriteLine($"{_kitchenFiles.ModName} has been paked successfully.. !");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }
}