using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;
using tot.Services;

namespace Tot.Commands;

public class CleanCommand : ITotCommand, ITotCommandInvoked, ITotCommandOptions
{
    public string Command => "clean";
    public string Description => "Clean any missing file from the cookinfo.ini";
    
    public string ModName { get; set; } = string.Empty;

    public IEnumerable<Option> GetOptions()
    {
        yield return Utils.GetModNameOption(x => ModName = x);
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var clerk = provider.GetRequiredService<KitchenClerk>();
        var kFiles = provider.GetRequiredService<KitchenFiles>();
        var console = provider.GetRequiredService<IColoredConsole>();

        try
        {
            kFiles.SetModName(ModName);
            var cookInfos = await clerk.GetCookInfo();
            var changes = clerk.RemoveMissingFiles(cookInfos);
            await clerk.SetCookInfo(cookInfos);
            foreach (var file in changes)
                Console.WriteLine(file);
            Console.WriteLine($"{changes.Count} missing file(s) removed from {kFiles.ModName} cookinfo.ini");
        }
        catch (CommandException ex)
        {
            return await console.OutputCommandError(ex);
        }

        return 0;
    }


}