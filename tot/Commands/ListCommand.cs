using System.CommandLine;
using System.Xml;
using Pastel;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ListCommand(IConsole console, KitchenFiles files) : IInvokableCommand<ListCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<ListCommand>("list", "List the mods available in the DevKit")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        foreach (var directory in files.ModsFolder.GetDirectories())
        {
            try
            {
                files.SetModName(directory.Name);
                var status = await files.GetModStatus();
                var infos = await files.GetModInfos();
                var stringDate =
                    $"{status.LastActionDate.ToLocalTime().ToShortDateString()} {status.LastActionDate.ToLocalTime().ToShortTimeString()}";
                console.WriteLine($"[{infos.SteamWorkshopFileIds.MainClient}] {directory.Name} - {infos.Name}");
                if (status.WasUploaded)
                    console.WriteLine($"    [{stringDate}] Uploaded"
                            .Pastel(Constants.ColorBlue));
                else if(status.WasSuccess)
                    console.WriteLine($"    [{stringDate}] Successfully Cooked"
                        .Pastel(Constants.ColorGreen));
                else
                    console.WriteLine($"    [{stringDate}] Failed to cook with {status.ErrorLogs.Count} Error(s)"
                        .Pastel(Constants.ColorRed));
            }
            catch
            {
                console.WriteLine(directory.Name);
            }
        }
        return 0;
    }
}
