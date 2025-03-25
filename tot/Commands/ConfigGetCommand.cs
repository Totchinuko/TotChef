using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;

namespace Tot.Commands;

public class ConfigGetCommand : ITotCommand, ITotCommandArguments, ITotCommandInvoked
{
    public string Command => "get";
    public string Description => "get the value of a config";
    
    public string Key { get; set; } = string.Empty;
    
    public Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var config = provider.GetRequiredService<Config>();
        
        if (string.IsNullOrEmpty(Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        console.WriteLine(config.GetValue(Key));
        return Task.FromResult(0);
    }

    public IEnumerable<Argument> GetArguments()
    {
        var arg = new TotArgument<string>("key", "Key of the config to interact with");
        arg.AddSetter(x => Key = x ?? string.Empty);
        yield return arg;
    }
}