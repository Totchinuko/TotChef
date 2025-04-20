using System.CommandLine;
using tot_lib.CommandLine;

namespace Tot.Commands;

public class PathCommand : ICommand<PathCommand>
{
    public static readonly Command Command = CommandBuilder
        .Create<PathCommand>("path", "Return a path to be used with cd")
        .SubCommands.Add(PathModCommand.Command)
        .SubCommands.Add(PathPakCommand.Command)
        .SubCommands.Add(PathSharedCommand.Command)
        .BuildCommand();

}

