using System.CommandLine;
using tot_lib;
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
                    .Colorize(ConsoleColors.BLUE));
            else if(status.WasSuccess)
                console.WriteLine($"{ModName} [Success][{stringDate}]"
                    .Colorize(ConsoleColors.GREEN));
            else
            {
                console.WriteLine($"{ModName} [Failed][{stringDate}][{status.ErrorLogs.Count} Error(s)]"
                    .Colorize(ConsoleColors.RED));
                foreach(string error in status.ErrorLogs)
                    console.WriteLine($"    {error}".Colorize(ConsoleColors.RED));
            }
        }
        catch
        {
            console.WriteLine("No Status");
        }
        return 0;
    }
}
