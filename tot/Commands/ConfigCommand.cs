using System.Reflection;
using CommandLine;
using LibGit2Sharp;

namespace Tot.Commands;

[Verb("config", HelpText = "Modify a config")]
public class ConfigCommand : ModBasedCommand, ICommand
{
    [Value(0, HelpText = "action to perform (get|set|list)", Required = true)]
    public string Action { get; set; } = string.Empty;
    
    [Value(1, HelpText = "Key of the config to interact with")]
    public string Key { get; set; } = string.Empty;
    
    [Value(1, HelpText = "Value of the config to interact with")]
    public string Value { get; set; } = string.Empty;
    
    public CommandCode Execute()
    {
        if (string.IsNullOrEmpty(Action))
            return CommandCode.MissingArg("action");
        
        var config = Config.LoadConfig();
        switch (Action)
        {
            case "get":
                return GetAction(config);
            case "set":
                return SetAction(config);
            case "list":
                return ListAction(config);
            default:
                return CommandCode.MissingArg("action");
        }
    }

    private CommandCode ListAction(Config config)
    {
        Tools.WriteColoredLine("Config List:", ConsoleColor.White);
        foreach (var prop in config.GetType().GetProperties())
        {
            if(prop.GetSetMethod() is null) continue;
            Tools.WriteColoredLine($"{prop.Name}={prop.GetValue(config)?.ToString() ?? "NULL"}", ConsoleColor.White);
        }
        return CommandCode.Success();
    }

    private CommandCode SetAction(Config config)
    {
        if(string.IsNullOrEmpty(Key))
            return CommandCode.MissingArg("key");
        var prop = config.GetType().GetProperty(Key);
        if(prop is null || prop.GetSetMethod() is null)
            return CommandCode.Error("unknown config key");
        prop.SetValue(config, Convert.ChangeType(Value, prop.PropertyType));
        config.SaveConfig();
        return CommandCode.Success();
    }

    private CommandCode GetAction(Config config)
    {
        if(string.IsNullOrEmpty(Key))
            return CommandCode.MissingArg("key");
        var prop = config.GetType().GetProperty(Key);
        if(prop is null || prop.GetSetMethod() is null)
            return CommandCode.Error("unknown config key");
        Tools.WriteColoredLine(prop.GetValue(config)?.ToString() ?? "NULL", ConsoleColor.White);
        return CommandCode.Success();
    }
}