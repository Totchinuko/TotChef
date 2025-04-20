using System.CommandLine;
using tot_lib;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class ConfigCommand : ICommand<ConfigCommand>
{
    public static Command Command = CommandBuilder
        .Create<ConfigCommand>("config", "Configure the cli settings")
        .SubCommands.Add(ConfigListCommand.Command)
        .SubCommands.Add(ConfigGetCommand.Command)
        .SubCommands.Add(ConfigSetCommand.Command)
        .BuildCommand();
}