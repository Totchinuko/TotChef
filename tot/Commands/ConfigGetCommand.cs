using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using tot_lib;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigGetCommand(IColoredConsole console, Config config) : IInvokableCommand<ConfigGetCommand>
{
    public static Command Command = CommandBuilder
        .CreateInvokable<ConfigGetCommand>("get", "get the value of a config")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("key", "Key of the config to interact with")
        .AddSetter((c, v) => c.Key = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    
    public string Key { get; set; } = string.Empty;
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Key))
            return console.OutputCommandError(CommandCode.MissingArg("key"));
        console.WriteLine(config.GetValue(Key));
        return Task.FromResult(0);
    }
}