using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigGetCommand(IConsole console, ILogger<ConfigGetCommand> logger, Config config) : IInvokableCommand<ConfigGetCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<ConfigGetCommand>("get", "get the value of a config")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("key", "Key of the config to interact with")
        .SetSetter((c, v) => c.Key = v ?? string.Empty).BuildArgument()
        .BuildCommand();
    
    public string Key { get; set; } = string.Empty;
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Key))
        {
            logger.LogCritical("Missing argument {arg}", "key");
            return Task.FromResult(1);
        }
        console.WriteLine(config.GetValue(Key));
        return Task.FromResult(0);
    }
}