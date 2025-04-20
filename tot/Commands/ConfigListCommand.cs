using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigListCommand(IColoredConsole console, Config config) : IInvokableCommand<ConfigListCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<ConfigListCommand>("list", "list all the config available and their current values")
        .SetServiceConfiguration(Program.ConfigureServices)
        .BuildCommand();
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        console.WriteLine("Listing all configs");
        foreach (var prop in config.GetKeyList()) 
            console.WriteLine($"{prop}={config.GetValue(prop)}");
        return Task.FromResult(0);
    }
}