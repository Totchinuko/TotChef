using System.CommandLine;
using tot_lib;

namespace Tot.Commands;

public class ConfigCommand : Command<ConfigCommandOptions, ConfigCommandHandler>
{
    public ConfigCommand() : base("config", "Modify a config")
    {
        var arg = new Argument<string>("action", "action to perform (get|set|list)");
        AddArgument(arg);
        arg = new Argument<string>("key", "Key of the config to interact with");
        AddArgument(arg);
        arg = new Argument<string>("value", "Value of the config to interact with");
        arg.SetDefaultValue("");
        AddArgument(arg);
    }
}

public class ConfigCommandOptions : ICommandOptions
{
    public string Action { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ConfigCommandHandler(IConsole console) : ICommandOptionsHandler<ConfigCommandOptions>
{
    public async Task<int> HandleAsync(ConfigCommandOptions options, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(options.Action))
            return await console.OutputCommandError(CommandCode.MissingArg("action"));

        var config = Config.LoadConfig();
        switch (options.Action)
        {
            case "get":
                return await GetAction(config, options);
            case "set":
                return await SetAction(config, options);
            case "list":
                return await ListAction(config, options);
            default:
                return await console.OutputCommandError(CommandCode.MissingArg("action"));
        }
    }

    private Task<int> ListAction(Config config, ConfigCommandOptions options)
    {
        console.WriteLine("Listing all configs");
        foreach (var prop in config.GetKeyList()) console.WriteLine($"{prop}={config.GetValue(prop)}");
        return Task.FromResult(0);
    }

    private Task<int> SetAction(Config config, ConfigCommandOptions options)
    {
        if (string.IsNullOrEmpty(options.Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        config.SetValue(options.Key, options.Value);
        config.SaveConfig();
        return Task.FromResult(0);
    }

    private Task<int> GetAction(Config config, ConfigCommandOptions options)
    {
        if (string.IsNullOrEmpty(options.Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        console.WriteLine(config.GetValue(options.Key));
        return Task.FromResult(0);
    }
}