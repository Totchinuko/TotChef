using System.CommandLine;
using Microsoft.Extensions.Logging;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigSetCommand(ILogger<ConfigSetCommand> logger, Config config) : IInvokableCommand<ConfigSetCommand>
{
    public static readonly Command Command = CommandBuilder
        .CreateInvokable<ConfigSetCommand>("set", "set a config")
        .SetServiceConfiguration(Program.ConfigureServices)
        .Arguments.Create<string>("key", "Key of the config to interact with")
        .SetSetter((c,v) => c.Key = v ?? string.Empty)
        .BuildArgument()
        .Arguments.Create<string>("value", "Value of the config to interact with")
        .SetSetter((c,v) => c.Value = v ?? string.Empty)
        .BuildArgument()
        .BuildCommand();
    
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    
    public Task<int> InvokeAsync(CancellationToken token)
    {
        if (string.IsNullOrEmpty(Key))
        {
            logger.LogCritical("Missing argument {arg}", "key");
            return Task.FromResult(1);
        }
        config.SetValue(Key, Value);
        config.SaveConfig();
        return Task.FromResult(0);
    }
}