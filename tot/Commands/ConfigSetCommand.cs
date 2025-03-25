using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;

namespace Tot.Commands;

public class ConfigSetCommand : ITotCommand, ITotCommandArguments, ITotCommandInvoked
{
    public string Command => "set";
    public string Description => "set a config";
    
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    public Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var config = provider.GetRequiredService<Config>();
        
        if (string.IsNullOrEmpty(Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        config.SetValue(Key, Value);
        config.SaveConfig();
        return Task.FromResult(0);
    }


    public IEnumerable<Argument> GetArguments()
    {
        var arg = new TotArgument<string>("key", "Key of the config to interact with");
        arg.AddSetter(x => Key = x ?? string.Empty);
        yield return arg;
        arg = new TotArgument<string>("value", "Value of the config to interact with");
        arg.AddSetter(x => Value = x ?? string.Empty);
        arg.SetDefaultValue("");
        yield return arg;
    }
}