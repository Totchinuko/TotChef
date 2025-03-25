using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;

namespace Tot.Commands;

public class ConfigCommand : ITotCommand, ITotCommandArguments
{
    public string Action { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public string Command => "config";
    public string Description => "Configure the cli settings";

    public IEnumerable<Argument> GetArguments()
    {
        var arg = new TotArgument<string>("action", "action to perform (get|set|list)");
        arg.AddSetter(x => Action = x ?? string.Empty);
        yield return arg;
        arg = new TotArgument<string>("key", "Key of the config to interact with");
        arg.AddSetter(x => Key = x ?? string.Empty);
        yield return arg;
        arg = new TotArgument<string>("value", "Value of the config to interact with");
        arg.AddSetter(x => Value = x ?? string.Empty);
        arg.SetDefaultValue("");
        yield return arg;
    }
    
    public async Task<int> InvokeAsync(IServiceProvider provider, CancellationToken token)
    {
        var console = provider.GetRequiredService<IColoredConsole>();
        var config = provider.GetRequiredService<Config>();
        
        if (string.IsNullOrEmpty(Action))
            return await console.OutputCommandError(CommandCode.MissingArg("action"));

        switch (Action)
        {
            case "get":
                return await GetAction(config, console);
            case "set":
                return await SetAction(config, console);
            case "list":
                return await ListAction(config, console);
            default:
                return await console.OutputCommandError(CommandCode.MissingArg("action"));
        }
    }

    private Task<int> ListAction(Config config, IColoredConsole console)
    {
        console.WriteLine("Listing all configs");
        foreach (var prop in config.GetKeyList()) console.WriteLine($"{prop}={config.GetValue(prop)}");
        return Task.FromResult(0);
    }

    private Task<int> SetAction(Config config, IColoredConsole console)
    {
        if (string.IsNullOrEmpty(Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        config.SetValue(Key, Value);
        config.SaveConfig();
        return Task.FromResult(0);
    }

    private Task<int> GetAction(Config config, IColoredConsole console)
    {
        if (string.IsNullOrEmpty(Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        console.WriteLine(config.GetValue(Key));
        return Task.FromResult(0);
    }
    
}