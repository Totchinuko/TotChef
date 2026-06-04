using System.CommandLine;
using System.Xml;
using Pastel;
using tot_lib.CommandLine;
using tot.Services;

namespace Tot.Commands;

public class ModStatusCommand(IConsole console, KitchenFiles files) : IInvokableCommand<ModStatusCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<ModStatusCommand>("mod-status", "Display the latest status for the mod")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Options.AddModName((c, v) => c.ModName = v)
        .BuildCommand();
    
    public string ModName { get; set; } = string.Empty;
    
    public async Task<int> InvokeAsync(CancellationToken token)
    {
        try
        {
            files.SetModName(ModName);
            var status = await files.GetModStatus();
            var stringDate =
                $"{status.LastActionDate.ToLocalTime().ToShortDateString()} {status.LastActionDate.ToLocalTime().ToShortTimeString()}";
            if (status.WasUploaded)
                console.WriteLine($"{ModName} [Uploaded][{stringDate}]"
                    .Pastel(Constants.ColorBlue));
            else if(status.WasSuccess)
                console.WriteLine($"{ModName} [Success][{stringDate}]"
                    .Pastel(Constants.ColorGreen));
            else
            {
                console.WriteLine($"{ModName} [Failed][{stringDate}][{status.ErrorLogs.Count} Error(s)]"
                    .Pastel(Constants.ColorRed));
                foreach(string error in status.ErrorLogs)
                    console.WriteLine($"    {error}".Pastel(Constants.ColorRed));
            }
        }
        catch
        {
            console.WriteLine("No Status");
        }
        return 0;
    }
}
