using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class CleanCommand(KitchenClerk clerk, KitchenFiles files, ILogger<CleanCommand> logger) : IInvokableCommand<CleanCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<CleanCommand>("clean", "Clean any missing file from the cookinfo.ini")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;

    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            var cookInfos = await clerk.GetCookInfo();
            var changes = clerk.RemoveMissingFiles(cookInfos);
            await clerk.SetCookInfo(cookInfos);
            foreach (var file in changes)
                Console.WriteLine(file);
            Console.WriteLine($"{changes.Count} missing file(s) removed from {files.ModName} cookinfo.ini");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Clean Failed");
            return ex.GetErrorCode();
        }

        return 0;
    }
}