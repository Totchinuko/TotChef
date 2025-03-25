using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;

namespace Tot.Commands;

public class ConfigListCommand : ITotCommand, ITotCommandInvoked
{
    public string Command => "list";
    public string Description => "list all the config available and their current values";
    
    public Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var config = provider.GetRequiredService<Config>();
        
        console.WriteLine("Listing all configs");
        foreach (var prop in config.GetKeyList()) 
            console.WriteLine($"{prop}={config.GetValue(prop)}");
        return Task.FromResult(0);
    }
}