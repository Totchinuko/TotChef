using System.CommandLine;
using System.Drawing;
using Pastel;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigListCommand(IConsole console, Config config) : IInvokableCommand<ConfigListCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<ConfigListCommand>("list", "list all the config available and their current values")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        console.WriteLine("Config list:".Pastel(Constants.ColorBlue));
        foreach (var prop in config.GetKeyList()) 
            console.WriteLine(
                prop.Pastel(Constants.ColorOrange) + 
                "=".Pastel(Constants.ColorGrey) + 
                config.GetValue(prop));
        return Task.FromResult(0);
    }
}