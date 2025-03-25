using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using tot_lib;

namespace Tot.Commands;

public class ConfigCommand : ITotCommand, ITotCommandSubCommands
{
    public string Command => "config";
    public string Description => "Configure the cli settings";

    public IEnumerable<ITotCommand> GetSubCommands()
    {
        yield return new ConfigGetCommand();
        yield return new ConfigSetCommand();
        yield return new ConfigListCommand();
    }
}