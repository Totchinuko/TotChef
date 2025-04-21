using System.CommandLine;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class PakCommand(KitchenFiles files, ILogger<PakCommand> logger) : IInvokableCommand<PakCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<PakCommand>("pak", "Pak the previously cooked files")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.Create<bool>("--compress", "Compress the files to reduce the final mod size").AddAlias("-c")
        .SetSetter((c,v) => c.Compress = v).BuildOption()
        .Options.AddModName((c,v) => c.ModName = v)
        .BuildCommand();
    
    public bool Compress { get; set; }

    public string ModName { get; set; } = string.Empty;
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);

            files.CreateModPakBackup();
            foreach (var file in files.ModFolder.GetFiles())
                if (!file.Name.StartsWith(".") && file.Name != "active.txt")
                    file.CopyTo(Path.Join(files.ModCookedFolder.FullName, file.Name), true);

            logger.LogInformation($"Paking {files.ModName}..");
            var p = Process.Start(
                files.UnrealPak.FullName,
                string.Join(" ",
                    files.ModPakFile.FullName,
                    "-Create=" + files.ModCookedFolder.FullName,
                    Compress ? "-compress" : ""
                ));
            await p.WaitForExitAsync(token);
            if(!p.HasExited) p.Kill();
            logger.LogInformation($"{files.ModName} has been paked successfully.. !");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to pak");
            return ex.GetErrorCode();
        }

        return 0;
    }
}