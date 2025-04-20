using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigSetCommand(IColoredConsole console, Config config) : IInvokableCommand<ConfigSetCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<ConfigSetCommand>("set", "set a config")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("key", "Key of the config to interact with")
        .AddSetter((c,v) => c.Key = v ?? string.Empty)
        .BuildArgument()
        .Arguments.Create<string>("value", "Value of the config to interact with")
        .AddSetter((c,v) => c.Value = v ?? string.Empty)
        .BuildArgument()
        .BuildCommand();
    
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        config.SetValue(Key, Value);
        config.SaveConfig();
        return Task.FromResult(0);
    }
}